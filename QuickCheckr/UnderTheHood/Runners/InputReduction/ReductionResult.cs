namespace QuickCheckr.UnderTheHood.Runners.InputReduction;

public record ReductionResult(bool Failed, object? Value)
{
    public bool Passed => !Failed;
    public T ValueAs<T>() => (T)Value!;
};
