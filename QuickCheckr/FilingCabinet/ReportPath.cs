using QuickCheckr.UnderTheHood;

namespace QuickCheckr.FilingCabinet;


public static class ReportPath
{
    public const string MainFolder = ".quickcheckr";
    public static string Main(CheckrRunnerConfig config) =>
        Path.Combine(MainFolder, config.FileAs, "report.qr");
}