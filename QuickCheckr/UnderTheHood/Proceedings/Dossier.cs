using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Proceedings;

public record Dossier(
    FailureInfo FailureInfo,
    RunInfo RunInfo,
    IReadOnlyDictionary<string, int> PassedExpectations,
    bool UseMemoryForInputReporting,
    ReportMode ReportMode,
    WarningLevel WarningLevel)
{
    public static Dossier Empty =>
        new(FailureInfo: new FailureInfo(),
            RunInfo: new RunInfo(),
            PassedExpectations: new Dictionary<string, int>(),
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.None,
            WarningLevel: WarningLevel.None);
}
