namespace QuickCheckr.UnderTheHood.Proceedings;

public record RunInfo(
    int OriginalRunExecutionCount = 0,
    int ShrinkCount = 0,
    int Seed = 0);
