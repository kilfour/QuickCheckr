using QuickCheckr.FilingCabinet;

namespace QuickCheckr.Protocol;

public static class DeliberationPolicy
{
    public static readonly Func<ICaseFileSummary, int> MinimumExecutions =
        a => a.ExecutionCount;

    public static readonly Func<ICaseFileSummary, int> MinimumOriginalExecutions =
        a => a.OriginalRunExecutionCount;

    public static readonly Func<ICaseFileSummary, int> MinimumWarnings =
        a => a.WarningCount;

    public static readonly Func<ICaseFileSummary, int> MinimumShrinkCount =
        a => a.ShrinkCount;

    public static readonly Func<ICaseFileSummary, int> MaximumShrinkCount =
        a => 0 - a.ShrinkCount;

    public static readonly Func<ICaseFileSummary, int> MaximumExecutionsShrunk =
        a => 0 - (a.OriginalRunExecutionCount - a.ExecutionCount);

    public static readonly Func<ICaseFileSummary, int> MaximumDistinctPassedExpectations =
        a => 0 - a.PassedExpectationCount;
}
