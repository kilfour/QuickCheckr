using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"A single collapser can expose several members. Each replacement receives the
latest reconstructed parent, so collapses made to earlier members are retained.")]
public class C_MultipleMembers : CheckrTest<C_MultipleMembers>
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
        from manifest in Checkr.Input(
            "Manifest",
            Fuzzr.Constant(new Manifest(
                [1, 42, 2],
                ["noise", "required", "extra"])),
            Shrink.OnType<Manifest>(
                collapse => collapse.Using(
                    collapse.Member(
                        manifest => manifest.Codes,
                        (manifest, codes) => new Manifest(codes, manifest.Tags)),
                    collapse.Member(
                        manifest => manifest.Tags,
                        (manifest, tags) => new Manifest(manifest.Codes, tags)))))
        from expect in Checkr.Expect(
            "Required code and tag cannot occur together",
            () => !(manifest.Codes.Contains(42) && manifest.Tags.Contains("required")))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(
            "Required code and tag cannot occur together",
            article.FailureDescription());
        Assert.Equal(
            "{ Codes: [ 42 ], Tags: [ \"required\" ] }",
            article.Execution(1).Input(1).Read().Value);

        var collapsed = Assert.IsType<Manifest>(
            article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal([42], collapsed.Codes);
        Assert.Equal(["required"], collapsed.Tags);
    }

    public sealed class Manifest(
        IEnumerable<int> codes,
        IEnumerable<string> tags)
    {
        public List<int> Codes { get; } = [.. codes];
        public List<string> Tags { get; } = [.. tags];
    }
}
