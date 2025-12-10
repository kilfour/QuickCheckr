namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public record ShrinkingInfo(
        int executionId,
        string key,
        object? value,
        string fullKey);
