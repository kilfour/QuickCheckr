namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public record ShrinkingInfo(
        int ExecutionId,
        string Key,
        object? Value,
        string FullKey,
        Type Type);
