using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Verification.StructureShrinking;

public class ReconstructingMemberCollapserTests
{
    [Fact]
    public void Collapses_a_getter_only_collection_by_reconstructing_its_parent()
    {
        var original = new Box([1, 42, 2]);

        var article = Run(
            original,
            box => !box.Values.Contains(42));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Values: [ 42 ] }", input.Value);
        var collapsed = Assert.IsType<Box>(input.InputValue);
        Assert.Equal([42], collapsed.Values);
        Assert.NotSame(original, collapsed);
        Assert.Equal([1, 42, 2], original.Values);
    }

    [Fact]
    public void Composes_reconstructing_collapsers_for_nested_getter_only_members()
    {
        var original = new OuterBox(new Box([1, 42, 2]));
        var checkr =
            from outer in Checkr.Input(
                "Outer Box",
                Fuzzr.Constant(original),
                Shrink.OnType<OuterBox>(
                    collapse => collapse.Using(
                        collapse.Member(
                            outer => outer.Inner,
                            (outer, inner) => new OuterBox(inner)))),
                BoxCollapser)
            from expect in Checkr.Expect(
                "Expectation",
                () => !outer.Inner.Values.Contains(42))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Inner: { Values: [ 42 ] } }", input.Value);
        var collapsed = Assert.IsType<OuterBox>(input.InputValue);
        Assert.Equal([42], collapsed.Inner.Values);
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
                    collapse => collapse.Using(
                        collapse.Member(
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
        var collapsed = Assert.IsType<MixedBox>(input.InputValue);
        Assert.Equal(42, collapsed.Code);
        Assert.Equal([42], collapsed.Values);
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
                    collapse => collapse.Using(
                        collapse.Member(
                            request => request.Code,
                            (request, code) => new Request(code, request.Note)))))
            from expect in Checkr.Expect(
                "Expectation",
                () => request.Code != 42)
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Code: 42, Note: { Text: \"retained\" } }", input.Value);
        var retained = Assert.IsType<Request>(input.InputValue);
        Assert.Same(note, retained.Note);
    }

    private static Article Run(Box original, Func<Box, bool> expectation)
    {
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(original),
                BoxCollapser)
            from expect in Checkr.Expect("Expectation", () => expectation(box))
            select Case.Closed;

        return Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));
    }

    private static readonly Shrinker BoxCollapser =
        Shrink.OnType<Box>(
            collapse => collapse.Using(
                collapse.Member(
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
