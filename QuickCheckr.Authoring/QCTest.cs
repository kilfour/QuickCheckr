using System.Runtime.CompilerServices;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Explains;

namespace QuickCheckr.Authoring;

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
    protected class DocFuzzrHeaderAttribute() :
        DocBoldHeaderAttribute("The Fuzzr");
    protected class DocFileCodeHeaderAttribute(string code)
    : DocFileHeaderAttribute(code.Replace("<", "&lt;").Replace(">", "&gt;"));
}

public abstract class QCTest<T> : QCTest
{
    protected virtual bool WriteAllReportsToDisk => false;
    protected virtual bool Asserts => false;
    protected virtual bool Report => false;
    protected virtual bool Explain => false;

    protected virtual Func<CaseFile, Flow<Flow>> StyleGuide => The.CourtStyleGuide;

    protected class DocReport([CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", 0, -1, path);

    protected class DocReportExcerpt(int skiplines = 0, int numberOfLines = -1, [CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", skiplines, numberOfLines, path);

    protected virtual CheckrConfig Configure(CheckrConfig config) => config;

    protected void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
        Verify(article);
    }

    protected abstract void Verify(Article article);

    private void WriteReport(
        Article article,
        string callerPath)
            => DoTheWriting(article, typeof(T).Name + ".txt", callerPath);

    private void DoTheWriting(Article article, string filename, string callerPath)
    {
        if (!Report && !WriteAllReportsToDisk) return;
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
        var report = TheClerk.Transcribes(article.CaseFile, StyleGuide);
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
