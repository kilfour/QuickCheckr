namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public record ReductionResult(bool Failed, object? Value)
{
    public bool Passed => !Failed;
    public T ValueAs<T>() => (T)Value!;
};
