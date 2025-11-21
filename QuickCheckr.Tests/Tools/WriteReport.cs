using System.Runtime.CompilerServices;
using QuickPulse.Arteries;

namespace QuickCheckr.Tests.Tools;

public static class WriteReport
{
    public static void ToFile(
        string filename,
        string report,
        [CallerFilePath] string callerPath = "")
    {
        var dir = Path.GetDirectoryName(callerPath)!;
        var fullPath = Path.Combine(dir, filename);
        FileLog.Write(fullPath).Absorb(report);
    }
}