namespace QuickCheckr.UnderTheHood.Proceedings;

internal sealed record RunInfo(
    int OriginalRunExecutionCount = 0,
    int ShrinkCount = 0,
    int Seed = 0);
