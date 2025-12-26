using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Protocol;

public static class DeliberationPolicy
{
    public static readonly Func<CaseFile, CaseFile, CaseFile> MinimumExecutions =
        (a, b) => a.Verdict!.ExecutionCount < b.Verdict!.ExecutionCount ? a : b;

    public static readonly Func<CaseFile, CaseFile, CaseFile> MinimumShrinkCount =
        (a, b) => a.Verdict!.ShrinkCount < b.Verdict!.ShrinkCount ? a : b;

    public static readonly Func<CaseFile, CaseFile, CaseFile> MaximumShrinkCount =
        (a, b) => a.Verdict!.ShrinkCount > b.Verdict!.ShrinkCount ? a : b;
    public static readonly Func<CaseFile, CaseFile, CaseFile> MinimumWarnings =
        (a, b) =>
            a.Verdict!.ExecutionDepositions.Sum(a => a.WarningDepositions.Count) <
            b.Verdict!.ExecutionDepositions.Sum(a => a.WarningDepositions.Count) ? a : b;

    public static readonly Func<CaseFile, CaseFile, CaseFile> MaximumExecutionsShrunk =
        (a, b) =>
            a.Verdict!.OriginalRunExecutionCount - a.Verdict!.ExecutionDepositions.Count >
            b.Verdict!.OriginalRunExecutionCount - b.Verdict!.ExecutionDepositions.Count ? a : b;

    public static readonly Func<CaseFile, CaseFile, CaseFile> MaximumDistinctPassedExpectations =
        (a, b) =>
            a.PassedExpectationDepositions.Count >
            b.PassedExpectationDepositions.Count ? a : b;
}
