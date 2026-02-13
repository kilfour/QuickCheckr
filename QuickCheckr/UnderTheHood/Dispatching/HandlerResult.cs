namespace QuickCheckr.UnderTheHood.Dispatching;

public static class HandlerResult
{
    public static HandlerResult<T> Unhandled<T>() => default;
    public static HandlerResult<T> Handled<T>(T output) => new(true, output);
}

public readonly record struct HandlerResult<T>
{
    private readonly T output;

    public HandlerResult(bool HasOutput, T output)
    {
        this.HasOutput = HasOutput;
        this.output = output;
    }

    public bool HasOutput { get; }
    public T Output => HasOutput
        ? output
        : throw new InvalidOperationException("HandlerResult has no output.");
}

