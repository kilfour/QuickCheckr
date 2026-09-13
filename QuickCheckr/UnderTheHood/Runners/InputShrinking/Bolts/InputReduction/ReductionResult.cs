namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

/// <summary>
/// Reports whether the previous reduction candidate failed and the value that was tested.
/// </summary>
public sealed record ReductionResult(bool Failed, object? Value)
{
    public bool Passed => !Failed;
    public T ValueAs<T>() => (T)Value!;
};
