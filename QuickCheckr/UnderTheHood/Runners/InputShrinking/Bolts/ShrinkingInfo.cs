namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public record ShrinkingInfo(
        int ExecutionId,
        string Key,
        object? Value,
        string Path,
        Type Type)
{
    public Type NormalizedType => Nullable.GetUnderlyingType(Type) ?? Type;
}
