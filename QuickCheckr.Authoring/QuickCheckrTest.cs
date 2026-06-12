using System.Runtime.CompilerServices;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;
using QuickPulse.Explains;

namespace QuickCheckr.Authoring;

public abstract class QuickCheckrTest<T> : DocAttributeBaseTest
{
    protected virtual bool WriteAllReportsToDisk => false;
    protected virtual bool Asserts => false;
    protected virtual bool PassedExpectationsContains => false;
    protected virtual bool Report => false;
    protected virtual bool Explain => false;

    protected virtual ITranscribe Clerk =>
        CourtClerk.Default();

    protected class DocReport([CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", 0, -1, path);

    protected class DocReportExcerpt(int skiplines = 0, int numberOfLines = -1, [CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", skiplines, numberOfLines, path);

    protected virtual void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
    }

    private void WriteReport(
        Article article,
        string callerPath)
            => DoTheWriting(article, typeof(T).Name + ".txt", callerPath);

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
                    SourceFile = $"{typeof(T).Name}.cs"
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

    private void ExplainMe()
    {
        if (!Explain) return;
        QuickPulse.Explains.Explain.OnlyThis<T>("temp-" + typeof(T).Name + ".md");
    }
}
