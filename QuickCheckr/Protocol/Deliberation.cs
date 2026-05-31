using QuickCheckr.FilingCabinet;

namespace QuickCheckr.Protocol;

public sealed record Deliberation(
    Func<CaseFile, int> Score,
    int? Target)
{
    public static Deliberation MinimumExecutions(int? target = null) =>
        new(a => a.ExecutionCount, target);

    public static Deliberation MinimumOriginalExecutions(int? target = null) =>
        new(a => a.OriginalRunExecutionCount, target);

    public static Deliberation MinimumWarnings(int? target = null) =>
        new(a => a.WarningCount, target);

    public static Deliberation MinimumShrinkCount(int? target = null) =>
        new(a => a.ShrinkCount, target);

    public static Deliberation MaximumShrinkCount(int? target = null) =>
        new(a => 0 - a.ShrinkCount, target);

    public static Deliberation MaximumExecutionsShrunk(int? target = null) =>
        new(a => 0 - (a.OriginalRunExecutionCount - a.ExecutionCount), target);

    public static Deliberation MaximumDistinctPassedExpectations(int? target = null) =>
        new(a => 0 - a.PassedExpectationCount, target);
}