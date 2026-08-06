using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Verification.StructureShrinking;

public class ReconstructingMemberTraversalTests
{
    [Fact]
    public void Traverses_a_getter_only_collection_by_reconstructing_its_parent()
    {
        var original = new Box([1, 42, 2]);

        var article = Run(
            original,
            box => !box.Values.Contains(42));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Values: [ 42 ] }", input.Value);
        var reconstructed = Assert.IsType<Box>(input.InputValue);
        Assert.Equal([42], reconstructed.Values);
        Assert.NotSame(original, reconstructed);
        Assert.Equal([1, 42, 2], original.Values);
    }

    [Fact]
    public void Composes_reconstructing_traversals_for_nested_getter_only_members()
    {
        var original = new OuterBox(new Box([1, 42, 2]));
        var checkr =
            from outer in Checkr.Input(
                "Outer Box",
                Fuzzr.Constant(original),
                Shrink.OnType<OuterBox>(
                    traversal => traversal.Using(
                        traversal.Member(
                            outer => outer.Inner,
                            (outer, inner) => new OuterBox(inner)))),
                BoxTraversal)
            from expect in Checkr.Expect(
                "Expectation",
                () => !outer.Inner.Values.Contains(42))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Inner: { Values: [ 42 ] } }", input.Value);
        var reconstructed = Assert.IsType<OuterBox>(input.InputValue);
        Assert.Equal([42], reconstructed.Inner.Values);
        Assert.Equal([1, 42, 2], original.Inner.Values);
    }

    [Fact]
    public void Uses_default_traversal_for_unconfigured_writable_members()
    {
        var checkr =
            from box in Checkr.Input(
                "Mixed Box",
                Fuzzr.Constant(new MixedBox([1, 42, 2]) { Code = 42 }),
                Shrink.OnType<MixedBox>(
                    traversal => traversal.Using(
                        traversal.Member(
                            box => box.Values,
                            (box, values) => new MixedBox(values) { Code = box.Code }))))
            from expect in Checkr.Expect(
                "Expectation",
                () => !(box.Code == 42 && box.Values.Contains(42)))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Code: 42, Values: [ 42 ] }", input.Value);
        var reconstructed = Assert.IsType<MixedBox>(input.InputValue);
        Assert.Equal(42, reconstructed.Code);
        Assert.Equal([42], reconstructed.Values);
    }

    [Fact]
    public void Retains_unconfigured_read_only_members_as_opaque_values()
    {
        var note = new Note("retained");
        var checkr =
            from request in Checkr.Input(
                "Request",
                Fuzzr.Constant(new Request(42, note)),
                Shrink.OnType<Request>(
                    traversal => traversal.Using(
                        traversal.Member(
                            request => request.Code,
                            (request, code) => new Request(code, request.Note)))))
            from expect in Checkr.Expect(
                "Expectation",
                () => request.Code != 42)
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured
                .Configure(config => config with { WarningLevel = WarningLevel.Verbose })
                .Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Code: 42, Note: { Text: \"retained\" } }", input.Value);
        var retained = Assert.IsType<Request>(input.InputValue);
        Assert.Same(note, retained.Note);
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(
            "Read-only member of type 'Note' at 'Request.Note' has no configured traversal and was retained as an opaque value.",
            article.Execution(1).Warning(1).Read().Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Explicit_classifier_takes_precedence_over_same_type_traversal(
        bool traversalFirst)
    {
        var original = new Box([1, 42, 2]);
        var keep = Shrink.Keep<Box>();
        Shrinker[] shrinkers = traversalFirst
            ? [BoxTraversal, keep]
            : [keep, BoxTraversal];
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(original),
                shrinkers)
            from expect in Checkr.Expect(
                "Expectation",
                () => !box.Values.Contains(42))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Values: [ 1, 42, 2 ] }", input.Value);
        Assert.Same(original, input.InputValue);
    }

    [Fact]
    public void Traversal_uses_custom_classifiers_for_nested_member_values()
    {
        var classifierWasCalled = false;
        var classifyInts = Shrink.OnType<int>(
            classification => classification.Classify(
                Try.Function<int>(_ =>
                {
                    classifierWasCalled = true;
                    return [0];
                })));
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(new Box([1, 42, 2])),
                BoxTraversal,
                classifyInts)
            from expect in Checkr.Expect(
                "Expectation",
                () => !box.Values.Contains(42))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        Assert.True(classifierWasCalled);
        Assert.Equal(
            "{ Values: [ 42 ] }",
            article.Execution(1).Input(1).Read().Value);
    }

    private static Article Run(Box original, Func<Box, bool> expectation)
    {
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(original),
                BoxTraversal)
            from expect in Checkr.Expect("Expectation", () => expectation(box))
            select Case.Closed;

        return Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));
    }

    private static readonly Shrinker BoxTraversal =
        Shrink.OnType<Box>(
            traversal => traversal.Using(
                traversal.Member(
                    box => box.Values,
                    (box, values) => new Box(values))));

    private sealed class Box(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
    }

    private sealed class OuterBox(Box inner)
    {
        public Box Inner { get; } = inner;
    }

    private sealed class MixedBox(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
        public int Code { get; set; }
    }

    private sealed class Request(int code, Note note)
    {
        public int Code { get; } = code;
        public Note Note { get; } = note;
    }

    private sealed class Note(string text)
    {
        public string Text { get; } = text;
    }
}
