using QuickCheckr.FilingCabinet;

namespace QuickCheckr;

/// <summary>
/// Common run-level scoring functions for <see cref="Trackr.Deliberate"/>.
/// </summary>
public static class Deliberation
{
    public static Func<CaseFile, int> MinimumExecutions() =>
        a => a.ExecutionCount;

    public static Func<CaseFile, int> MinimumOriginalExecutions() =>
        a => a.OriginalRunExecutionCount;

    public static Func<CaseFile, int> MinimumWarnings() =>
        a => a.WarningCount;

    public static Func<CaseFile, int> MinimumShrinkCount() =>
        a => a.ShrinkCount;

    public static Func<CaseFile, int> MaximumShrinkCount() =>
        a => 0 - a.ShrinkCount;

    public static Func<CaseFile, int> MaximumExecutionsShrunk() =>
        a => 0 - (a.OriginalRunExecutionCount - a.ExecutionCount);

    public static Func<CaseFile, int> MaximumDistinctPassedExpectations() =>
        a => 0 - a.PassedExpectationCount;
}
