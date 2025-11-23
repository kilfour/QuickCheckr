using System.Runtime.CompilerServices;
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
    {
        if (!writeToDisk) return;
        var filename = typeof(T).Name + ".txt";
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        var report = TheClerk.Transcribes(article.CaseFile);
        var formattedReport = report
            .Replace(@$"{basePath}QuickCheckr\QuickCheckr.Tests\", "")
            .Replace(@$"{basePath}QuickCheckr\QuickCheckr\", "")
            .Replace(@"<RunTest>b__0_0", "Example");
        FileLog.Write(fullPath).Absorb(formattedReport);
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
