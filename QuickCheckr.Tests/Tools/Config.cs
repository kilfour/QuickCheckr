using QuickCheckr.Protocol;

namespace QuickCheckr.Tests.Tools;


public static class Config
{
    public static CheckrConfig NoLabels(CheckrConfig a)
        => a with { ReportMode = a.ReportMode & ~ReportMode.Labels };
}