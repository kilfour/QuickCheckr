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
    public class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");
    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");
    public class DocReport([CallerFilePath] string path = "") :
        DocCodeFileAttribute($"{typeof(T).Name}.txt", "text", 0, path);

    public abstract void Example();
    protected virtual CheckrConfig Configure(CheckrConfig config) => config;
    protected abstract CheckrOf<Case> TheCheckr();
    protected abstract void Verify(Article article);

    protected virtual bool Asserts => false;
    protected virtual bool Report => false;
    protected virtual bool Explanation => false;

    protected void Run(
        int seed,
        CheckrOfTRun.ExecutionCount executionCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, executionCount))
            , callerPath);

    protected void Run(
        int seed,
        CheckrOfTRun.ExecutionCount executionCount,
        Func<CheckrConfig, CheckrConfig> configure,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, executionCount, configure))
            , callerPath);

    private void ProcessArticle(Article article, string callerPath)
    {
        WriteAsserts(article);
        WriteReport(article, callerPath);
        ExplainMe();
        Verify(article);
    }

    private const string basePath = @"C:\Code\";
    private readonly bool writeAllReportsToDisk = false;

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
        // var formattedReport = report
        //     .Replace(@$"{basePath}QuickCheckr\QuickCheckr.Tests\", "")
        //     .Replace(@$"{basePath}QuickCheckr\QuickCheckr\", "")
        //     .Replace(@"<RunTest>b__0_0", "Example");
        // var cleaned = Regex.Replace(
        //     formattedReport,
        //     @"<(?<name>[^>]+)>b__[\d_]+",
        //     m => m.Groups["name"].Value);
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
