using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class Checkr_ToLinq
{
    public static CheckrOf<TValueTwo> Select<TValueOne, TValueTwo>(
        this CheckrOf<TValueOne> script,
        Func<TValueOne, TValueTwo> selector) =>
            async state =>
                {
                    if (state.RunStopped)
                        return CheckrResult.Default<TValueTwo>(state);
                    var result = await script(state).ConfigureAwait(false);
                    if (state.RunStopped)
                        return CheckrResult.Default<TValueTwo>(state);
                    return CheckrResult.Some(state, selector(result.Value));
                };

    public static CheckrOf<TResult> SelectMany<TSource, TResult>(
        this CheckrOf<TSource> source,
        Func<TSource, CheckrOf<TResult>> selector) =>
            async state =>
                {
                    if (state.RunStopped)
                        return CheckrResult.Default<TResult>(state);
                    var result = await source(state).ConfigureAwait(false);
                    if (state.RunStopped)
                        return CheckrResult.Default<TResult>(state);
                    return await selector(result.Value)(state).ConfigureAwait(false);
                };

    public static CheckrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this CheckrOf<TValueOne> script,
        Func<TValueOne, CheckrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
            => script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}
