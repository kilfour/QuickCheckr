using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.UnderTheHood.Protocol;

public static class DeliberationPolicy
{
    public static readonly Func<CaseFile, CaseFile, CaseFile> MinimumExecutions =
        (a, b) => a.Verdict!.ExecutionCount < b.Verdict!.ExecutionCount ? a : b;

    public static readonly Func<CaseFile, CaseFile, CaseFile> MinimumShrinkCount =
        (a, b) => a.Verdict!.ShrinkCount < b.Verdict!.ShrinkCount ? a : b;
}
