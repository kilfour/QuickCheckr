namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Represents a value produced by a conditional step, including the case where that step was skipped.
/// </summary>
public sealed class ConditionalResult<T>
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
    internal ConditionalResult(T? value) => this.value = value;
    private static ConditionalResult<T> NotExecuted() => new() { };

    internal static async ValueTask<CheckrResult<ConditionalResult<T>>> AsCheckrResultAsync(
        CheckrState state,
        Func<bool> predicate,
        CheckrOf<T> checkr) => predicate()
            ? CheckrResult.Some(state, new ConditionalResult<T>((await checkr(state).ConfigureAwait(false)).Value))
            : CheckrResult.Some(state, ConditionalResult<T>.NotExecuted());
}
