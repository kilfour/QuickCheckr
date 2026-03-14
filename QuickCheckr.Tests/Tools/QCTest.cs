using System.Runtime.CompilerServices;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class QCTest
{
    protected class DocBoldHeaderAttribute(string header) :
        DocContentAttribute($"\n**{header}:**");
    protected class DocModelHeaderAttribute() :
        DocBoldHeaderAttribute("The Model");
    protected class DocReportHeaderAttribute() :
        DocBoldHeaderAttribute("The Report");
    protected class DocReportExcerptHeaderAttribute() :
        DocBoldHeaderAttribute("Report Excerpt");
    protected class DocConfigureHeaderAttribute() :
        DocBoldHeaderAttribute("The Configuration");
    protected class DocCheckrHeaderAttribute() :
        DocBoldHeaderAttribute("The Checkr");
}

public abstract class QCTest<T> : QCTest
{
    protected readonly bool writeAllReportsToDisk = false;
    protected virtual bool Asserts => false;
    protected virtual bool Report => false;
    protected virtual bool Explain => false;

    protected class DocReport([CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", 0, -1, path);

    protected class DocReportExcerpt(int skiplines = 0, int numberOfLines = -1, [CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", skiplines, numberOfLines, path);

    public abstract void Example();

    protected void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
        Verify(article);
    }

    protected abstract void Verify(Article article);

    private const string basePath = @"C:\Code\";

    private void WriteReport(
        Article article,
        string callerPath)
            => DoTheWriting(article, typeof(T).Name + ".txt", callerPath);

    private void DoTheWriting(Article article, string filename, string callerPath)
    {
        if (!Report && !writeAllReportsToDisk) return;
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        if (article.CaseFile.HasEvidence)
        {
            article.CaseFile.AddTestMethodDisposition(
                article.CaseFile.TestMethodInfoDeposition! with
                {
                    MethodName = "Example",
                    SourceFile = $"{typeof(T).Name}.cs"
                });
        }
        var report = TheClerk.Transcribes(article.CaseFile);
        FileLog.Write(fullPath).Absorb(report);
    }

    private void WriteAsserts(Article article)
    {
        if (Asserts)
            TheJournalist.Transcribes(article);
    }

    private void ExplainMe()
    {
        if (!Explain) return;
        QuickPulse.Explains.Explain.OnlyThis<T>("temp-" + typeof(T).Name + ".md");
    }
}
