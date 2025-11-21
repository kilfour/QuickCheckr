using System.Runtime.CompilerServices;
using QuickPulse.Arteries;

namespace QuickCheckr.Tests.Tools;

public static class WriteReport
{
    private static readonly bool ToDisk = false;

    public static void ToFile(
        string filename,
        string report,
        [CallerFilePath] string callerPath = "")
    {
        if (!ToDisk) return;
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        var formattedReport = report
                .Replace(@"C:\Code\QuickCheckr\QuickCheckr.Tests\", "")
                .Replace(@"C:\Code\QuickCheckr\QuickCheckr\", "");
        FileLog.Write(fullPath).Absorb(formattedReport);
    }
}