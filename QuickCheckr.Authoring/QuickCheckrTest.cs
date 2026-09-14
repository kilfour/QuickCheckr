using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;
using QuickPulse.Explains;
using QuickPulse.Explains.Abstractions;

namespace QuickCheckr.Authoring;

public abstract class QuickCheckrTest<TTestClass> : DocAttributeBaseTest<TTestClass>
{
    protected virtual bool WriteAllReportsToDisk => false;
    protected virtual bool Asserts => false;
    protected virtual bool PassedExpectationsContains => false;
    protected virtual bool Report => false;

    protected virtual ITranscribe Clerk =>
        CourtClerk.Default();

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class DocReportAttribute([CallerFilePath] string path = "") : Attribute, IDocAttribute
    {
        public IEnumerable<DocFragmentAttribute> Expand()
        {
            yield return new DocBoldHeaderAttribute("The Report");
            yield return new DocCodeFileAttribute($"{typeof(TTestClass).Name}.txt", "text", 0, -1, path);
        }
    }

    protected class DocReportExcerpt(int skiplines = 0, int numberOfLines = -1, [CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(TTestClass).Name}.txt", "text", skiplines, numberOfLines, path);

    [StackTraceHidden]
    protected void Document(
        Action<Journalist> runCheckr,
        [CallerFilePath] string callerPath = "")
        => Document(runCheckr, _ => { }, callerPath);

    [StackTraceHidden]
    protected void Document(
        Action<Journalist> runCheckr,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        var journalist = new Journalist();
        try { runCheckr(journalist); }
        catch (FalsifiableException) { }
        var article = journalist.GetArticle();
        ProcessArticle(article, callerPath);
        verifier(article);
    }

    [StackTraceHidden]
    protected void Document(
        CheckrOf<Case> checkr,
        Action<ConfiguredCheckr> runCheckr,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        var article = Journalist.Publish(checkr, runCheckr);
        ProcessArticle(article, callerPath);
        verifier(article);
    }

    [StackTraceHidden]
    protected void Document(
        ConfiguredCheckr checkr,
        Action<ConfiguredCheckr> runCheckr,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        var article = Journalist.Publish(checkr, runCheckr);
        ProcessArticle(article, callerPath);
        verifier(article);
    }

    protected void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
    }

    private void WriteReport(
        Article article,
        string callerPath)
            => DoTheWriting(article, typeof(TTestClass).Name + ".txt", callerPath);

    private void DoTheWriting(Article article, string filename, string callerPath)
    {
        if (!Report && !WriteAllReportsToDisk) return;
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        if (article.CaseFile is not null && article.CaseFile.HasEvidence)
        {
            article.CaseFile.AddTestMethodDisposition(
                article.CaseFile.TestMethodInfoDeposition! with
                {
                    MethodName = "Example",
                    SourceFile = $"{typeof(TTestClass).Name}.cs"
                });
        }
        var report = TheClerk.Transcribes(article.Record, Clerk.File);
        FileLog.Write(fullPath).Absorb(report);
    }

    private void WriteAsserts(Article article)
    {
        if (Asserts)
            Journalist.Transcribe(article, PassedExpectationsContains);
    }
}
