using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"A configured collapser overlays the default object handling. Unconfigured writable
members use default traversal; unconfigured read-only members are retained as opaque values.")]
public class F_UnconfiguredReadOnlyMemberIsRetained
    : CheckrTest<F_UnconfiguredReadOnlyMemberIsRetained>
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
        from request in Checkr.Input(
            "Request",
            Fuzzr.Constant(new Request(42, new Note("irrelevant"))),
            Shrink.OnType<Request>(
                collapse => collapse.Using(
                    collapse.Member(
                        request => request.Code,
                        (request, code) => new Request(code, request.Note)))))
        from expect in Checkr.Expect(
            "Code is not 42",
            () => request.Code != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Code is not 42", article.FailureDescription());

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal(
            "{ Code: 42, Note: { Text: \"irrelevant\" } }",
            input.Value);

        var retained = Assert.IsType<Request>(input.InputValue);
        Assert.NotNull(retained.Note);
        Assert.Equal("irrelevant", retained.Note.Text);
    }

    public sealed class Request(int code, Note? note)
    {
        public int Code { get; } = code;
        public Note? Note { get; } = note;
    }

    public sealed class Note(string text)
    {
        public string Text { get; } = text;
    }
}
