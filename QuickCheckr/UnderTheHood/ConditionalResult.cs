namespace QuickCheckr.UnderTheHood;

public class ConditionalResult<T>
{
    private readonly bool notExecuted = false;
    private readonly T? value;
    public T? Value
    {
        get
        {
            if (notExecuted)
                throw new CheckrWasNotExecuted();
            return value;
        }
    }
    private ConditionalResult() { notExecuted = true; }
    public ConditionalResult(T? value) => this.value = value;
    private static ConditionalResult<T> NotExecuted() => new() { };

    public static CheckrResult<ConditionalResult<T>> AsCheckrResult(
        CheckrState state,
        Func<bool> predicate,
        Func<T?> factory) => predicate()
            ? CheckrResult.Some(state, new ConditionalResult<T>(factory()))
            : CheckrResult.Some(state, ConditionalResult<T>.NotExecuted());
}
