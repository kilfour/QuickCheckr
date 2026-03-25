using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Proceedings;

public record Dossier(
    FailureInfo FailureInfo,
    RunInfo RunInfo,
    IReadOnlyDictionary<string, int> PassedExpectations,
    List<string> UncheckedExpectations,
    bool UseMemoryForInputReporting,
    ReportMode ReportMode)
{
    public static Dossier Empty =>
        new(FailureInfo: new FailureInfo(),
            RunInfo: new RunInfo(),
            PassedExpectations: new Dictionary<string, int>(),
            UncheckedExpectations: [],
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.None);
}
