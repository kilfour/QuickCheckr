using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class ToLinq
{
	public static CheckrOf<TValueTwo> Select<TValueOne, TValueTwo>(
		this CheckrOf<TValueOne> script,
		Func<TValueOne, TValueTwo> selector) =>
			state =>
				{
					if (state.RunStopped)
						return CheckrResult.Default<TValueTwo>(state);
					return CheckrResult.Some(state, selector(script(state).Value));
				};

	public static CheckrOf<TResult> SelectMany<TSource, TResult>(
		this CheckrOf<TSource> source,
		Func<TSource, CheckrOf<TResult>> selector) =>
			state =>
				{
					if (state.RunStopped)
						return CheckrResult.Default<TResult>(state);
					var result = source(state);
					if (state.RunStopped)
						return CheckrResult.Default<TResult>(state);
					return selector(result.Value)(state);
				};

	public static CheckrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
		this CheckrOf<TValueOne> script,
		Func<TValueOne, CheckrOf<TValueTwo>> selector,
		Func<TValueOne, TValueTwo, TValueThree> projector)
			=> script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}
