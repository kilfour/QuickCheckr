using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;
using QuickTestr.Bolts.Builders.Strings;

namespace QuickTestr.Tests.Notes.T_Exploration;

[DocFile]
[DocBoldHeader("The StringTestr")]
[DocExample(typeof(StringTesting), nameof(GetStringTestr))]
[DocReportHeader]
[DocReport]
public class StringTesting : QuickCheckrTest<StringTesting>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => true;

    protected override ITranscribe Clerk => new StringTestrClerk();

    private readonly Journalist journalist = new();

    [StackTraceHidden]
    protected void Document(
        Action runTestr,
        [CallerFilePath] string callerPath = "")
    {
        try { runTestr(); }
        catch (FalsifiableException) { }
        var article = journalist.GetArticle();
        ProcessArticle(article, callerPath);
        Verify(article);
    }

    [Fact]
    public void Example() =>
        Document(() => GetStringTestr());

    [CodeSnippet]
    [CodeRemove(".StoreCaseFiles(journalist)")]
    public ConfiguredCheckr GetStringTestr() =>
        StringTestr
            .Named("Create BookTitle")
            .For(BookTitle.Create)
            .Accepts(Fuzzr.String(1, 100))
            .FailsWith<ComputerSaysNo>("Title is required.",
                StringCase.Null,
                StringCase.Empty,
                StringCase.Whitespace)
            .FailsWith<ComputerSaysNo>("Title cannot be longer than 100 characters.",
                StringCase.LongerThan(100))
            .StoreCaseFiles(journalist)
            .Explore();

    private void Verify(Article article)
    {
    }
}

