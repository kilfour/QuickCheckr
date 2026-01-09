using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Protocol;

public static class DeliberationPolicy
{
    public static readonly Func<CaseFile, int> MinimumExecutions =
        a => a.Verdict!.ExecutionCount;

    public static readonly Func<CaseFile, int> MinimumOriginalExecutions =
        a => a.Verdict!.OriginalRunExecutionCount;

    public static readonly Func<CaseFile, int> MinimumShrinkCount =
        a => a.Verdict!.ShrinkCount;

    public static readonly Func<CaseFile, int> MaximumShrinkCount =
        a => 0 - a.Verdict!.ShrinkCount;
    public static readonly Func<CaseFile, int> MinimumWarnings =
        a => a.Verdict!.ExecutionDepositions.Sum(a => a.WarningDepositions.Count);

    public static readonly Func<CaseFile, int> MaximumExecutionsShrunk =
        a => 0 - (a.Verdict!.OriginalRunExecutionCount - a.Verdict!.ExecutionDepositions.Count);

    public static readonly Func<CaseFile, int> MaximumDistinctPassedExpectations =
        a => 0 - a.PassedExpectationDepositions.Count;
}
