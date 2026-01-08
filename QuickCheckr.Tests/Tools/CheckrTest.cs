using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;


public abstract class CheckrTest<T>
{
    private readonly bool writeAllReportsToDisk = false;

    public class DocBoldHeaderAttribute(string header) :
        DocContentAttribute($"\n**{header}:**");

    public class DocModelHeaderAttribute() :
        DocBoldHeaderAttribute("The Model");

    public class DocConfigureHeaderAttribute() :
        DocBoldHeaderAttribute("The Configuration");
    public class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");

    public class DocCheckrHeaderAttribute() :
        DocBoldHeaderAttribute("The Checkr");
    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");

    public class DocReportHeaderAttribute() :
        DocBoldHeaderAttribute("The Report");
    public class DocReport([CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", 0, -1, path);

    public class DocReportExcerptHeaderAttribute() :
        DocBoldHeaderAttribute("Report Excerpt");
    public class DocReportExcerpt(int skiplines = 0, int numberOfLines = -1, [CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", skiplines, numberOfLines, path);

    public abstract void Example();
    protected virtual CheckrConfig Configure(CheckrConfig config) => config;
    protected abstract CheckrOf<Case> TheCheckr();
    protected abstract void Verify(Article article);

    protected virtual bool Asserts => false;
    protected virtual bool Report => false;
    protected virtual bool Explanation => false;

    [StackTraceHidden]
    protected void Run(
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        int seed,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        int seed,
        CheckrOfTRun.ExecutionCount executionCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, executionCount, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        CheckrOfTRun.RunCount runCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(runCount, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        CheckrOfTRun.RunCount runCount,
        CheckrOfTRun.ExecutionCount executionCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(runCount, executionCount, Configure))
            , callerPath);

    private void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
        Verify(article);
    }

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
        if (article.CaseFile.HasVerdict())
        {
            article.CaseFile.Verdict!.AddTestMethodDisposition(
                article.CaseFile.Verdict?.TestMethodInfoDeposition! with
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
        if (!Explanation) return;
        Explain.OnlyThis<T>("temp-" + typeof(T).Name + ".md");
    }
}
