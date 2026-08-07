using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"Traversals compose recursively. The outer traversal reconstructs the envelope,
while the inner traversal exposes and reconstructs its getter-only collection.")]
public class B_NestedReconstruction : CheckrTest<B_NestedReconstruction>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(configured => configured.Run(1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from envelope in Checkr.Input(
            "Envelope",
            Fuzzr.Constant(new Envelope(new Payload([1, 42, 2]))),
            Shrink.OnType<Envelope>(
                traversal => traversal.Using(
                    traversal.Member(
                        envelope => envelope.Payload,
                        (envelope, payload) => new Envelope(payload)))),
            Shrink.OnType<Payload>(
                traversal => traversal.Using(
                    traversal.Member(
                        payload => payload.Values,
                        (payload, values) => new Payload(values)))))
        from expect in Checkr.Expect(
            "Payload does not contain 42",
            () => !envelope.Payload.Values.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Payload does not contain 42", article.FailureDescription());
        Assert.Equal(
            "{ Payload: { Values: [ 42 ] } }",
            article.Execution(1).Input(1).Read().Value);

        var reconstructed = Assert.IsType<Envelope>(
            article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal([42], reconstructed.Payload.Values);
    }

    public sealed class Envelope(Payload payload)
    {
        public Payload Payload { get; } = payload;
    }

    public sealed class Payload(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
    }
}
