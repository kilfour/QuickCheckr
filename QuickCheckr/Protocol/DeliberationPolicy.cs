using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Protocol;

public static class DeliberationPolicy
{
    public static readonly Func<CaseFile, int> MinimumExecutions =
        a => a.ExecutionCount;

    public static readonly Func<CaseFile, int> MinimumOriginalExecutions =
        a => a.OriginalRunExecutionCount;

    public static readonly Func<CaseFile, int> MinimumWarnings =
        a => a.ExecutionDepositions.Sum(a => a.WarningDepositions.Count);

    public static readonly Func<CaseFile, int> MinimumShrinkCount =
        a => a.ShrinkCount;

    public static readonly Func<CaseFile, int> MaximumShrinkCount =
        a => 0 - a.ShrinkCount;

    public static readonly Func<CaseFile, int> MaximumExecutionsShrunk =
        a => 0 - (a.OriginalRunExecutionCount - a.ExecutionDepositions.Count);

    public static readonly Func<CaseFile, int> MaximumDistinctPassedExpectations =
        a => 0 - a.PassedExpectationDepositions.Count;
}
