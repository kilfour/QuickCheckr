namespace QuickCheckr.UnderTheHood;

public record CheckrResult<TValue>(CheckrState State, TValue Value);

public static class CheckrResult
{
	public static CheckrResult<TValue> Some<TValue>(CheckrState state, TValue value) =>
		new(state, value);

	public static CheckrResult<TValue> None<TValue>(CheckrState state) =>
		new(state, default!);

	public static CheckrResult<Case> CheckOnly(CheckrState state) =>
		Some(state, Case.Closed);
}
