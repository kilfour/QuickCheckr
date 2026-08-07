using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"A configured traversal overlays the default object handling. Unconfigured writable
members use default traversal. Unconfigured read-only members are retained as opaque values
and produce a verbose warning because QuickCheckr cannot prove them irrelevant.")]
public class F_UnconfiguredReadOnlyMemberIsRetained
    : CheckrTest<F_UnconfiguredReadOnlyMemberIsRetained>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(configured => configured
            .Configure(config => config with { WarningLevel = WarningLevel.Verbose })
            .Run(1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from request in Checkr.Input(
            "Request",
            Fuzzr.Constant(new Request(42, new Note("irrelevant"))),
            Shrink.OnType<Request>(
                traversal => traversal.Using(
                    traversal.Member(
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
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(
            "Read-only member of type 'Note' at 'Request.Note' has no configured traversal and was retained as an opaque value.",
            article.Execution(1).Warning(1).Read().Value);
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
