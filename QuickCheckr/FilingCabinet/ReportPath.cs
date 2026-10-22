using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.FilingCabinet;


internal static class ReportPath
{
    public const string MainFolder = ".quickcheckr";
    public static string Main(CheckrConfig config) =>
        Path.Combine(MainFolder, config.FileAs, "report.qr");
}
