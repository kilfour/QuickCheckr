using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record RunInfo(
    int OriginalRunExecutionCount = 0,
    int ExecutionCount = 0,
    int ShrinkCount = 0,
    int Seed = 0);

public record FailureInfo(
    string? FailingExpectation = null,
    string? Verification = null,
    Exception? Exception = null,
    bool IncludeStackTrace = false);

public record Dossier(
    FailureInfo FailureInfo,
    RunInfo RunInfo,
    IReadOnlyDictionary<string, int> PassedExpectations,
    bool UseMemoryForInputReporting,
    ReportMode ReportMode)
{
    public static Dossier Empty =>
        new(FailureInfo: new FailureInfo(),
            RunInfo: new RunInfo(),
            PassedExpectations: new Dictionary<string, int>(),
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.None);
}
