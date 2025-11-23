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
    const string basePath = @"C:\Code\";
    protected static void WriteReport<T>(
        Article article,
        bool writeToDisk = false,
        [CallerFilePath] string callerPath = "")
            => DoTheWriting(article, typeof(T).Name + ".txt", callerPath, writeToDisk);

    protected static void WriteReport(
        Article article,
        string filename,
        bool writeToDisk = false,
        [CallerFilePath] string callerPath = "")
            => DoTheWriting(article, filename, callerPath, writeToDisk);

    private static void DoTheWriting(Article article, string filename, string callerPath, bool writeToDisk)
    {
        if (!writeToDisk) return;
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

    protected static void WriteAsserts(
        Article article,
        bool writeToDisk = false)
    {
        if (writeToDisk)
            TheJournalist.Transcribes(article);
    }

    protected static void ExplainMe<T>()
        => Explain.OnlyThis<T>("temp.md");
}
