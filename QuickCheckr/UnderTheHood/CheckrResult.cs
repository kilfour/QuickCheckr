namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Carries a Checkr step's value together with the execution state passed to the next step.
/// </summary>
public sealed record CheckrResult<TValue>
{
    internal CheckrResult(CheckrState state, TValue value)
    {
        State = state;
        Value = value;
    }

    internal CheckrState State { get; }
    internal TValue Value { get; }

    /// <summary>
    /// Wraps a synchronous step result as an already completed awaitable result.
    /// </summary>
    public static implicit operator ValueTask<CheckrResult<TValue>>(CheckrResult<TValue> result)
        => new(result);
}

/// <summary>
/// Creates results for custom <see cref="CheckrOf{T}"/> steps.
/// </summary>
public static class CheckrResult
{
	public static CheckrResult<TValue> Some<TValue>(CheckrState state, TValue value) =>
		new(state, value);

	public static CheckrResult<TValue> Default<TValue>(CheckrState state) =>
		new(state, default!);

	public static CheckrResult<Case> CaseOnly(CheckrState state) =>
		Some(state, Case.Closed);
}
