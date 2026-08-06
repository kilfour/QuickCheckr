using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"Collapsers compose recursively. The outer collapser reconstructs the envelope,
while the inner collapser exposes and reconstructs its getter-only collection.")]
public class B_NestedReconstruction : CheckrTest<B_NestedReconstruction>
{
    protected override bool Report => true;

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
                collapse => collapse.Using(
                    collapse.Member(
                        envelope => envelope.Payload,
                        (envelope, payload) => new Envelope(payload)))),
            Shrink.OnType<Payload>(
                collapse => collapse.Using(
                    collapse.Member(
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

        var collapsed = Assert.IsType<Envelope>(
            article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal([42], collapsed.Payload.Values);
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
