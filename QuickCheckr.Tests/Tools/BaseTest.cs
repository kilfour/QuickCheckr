using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Tools;

public abstract class BaseTest
{
    private const string basePath = @"C:\Code\";
    private readonly bool writeAllToDisk = false;

    protected void WriteReport<T>(
        Article article,
        bool writeToDisk = false,
        [CallerFilePath] string callerPath = "")
            => DoTheWriting(article, typeof(T).Name + ".txt", callerPath, writeToDisk);

    protected void WriteReport(
        Article article,
        string filename,
        bool writeToDisk = false,
        [CallerFilePath] string callerPath = "")
            => DoTheWriting(article, filename, callerPath, writeToDisk);

    private void DoTheWriting(Article article, string filename, string callerPath, bool writeToDisk)
    {
        if (!writeToDisk && !writeAllToDisk) return;
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        var report = TheClerk.Transcribes(article.CaseFile);
        var formattedReport = report
            .Replace(@$"{basePath}QuickCheckr\QuickCheckr.Tests\", "")
            .Replace(@$"{basePath}QuickCheckr\QuickCheckr\", "")
            .Replace(@"<RunTest>b__0_0", "Example");
        var cleaned = Regex.Replace(
            formattedReport,
            @"<(?<name>[^>]+)>b__[\d_]+",
            m => m.Groups["name"].Value);
        FileLog.Write(fullPath).Absorb(cleaned);
    }

    protected void WriteAsserts(
        Article article,
        bool writeToDisk = false)
    {
        if (writeToDisk && !writeAllToDisk)
            TheJournalist.Transcribes(article);
    }

    protected static void ExplainMe<T>(bool writeToDisk = false)
    {
        if (!writeToDisk) return;
        Explain.OnlyThis<T>("temp-" + typeof(T).Name + ".md");
    }
}