namespace QuickCheckr.StringReduction.Nuts;

public static class Selectr_ToLinq
{
    public static SelectrOf<TValueTwo> Select<TValueOne, TValueTwo>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, TValueTwo> selector) =>
            state =>
                {
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TValueTwo>(state);
                    return SelectrResult.Some(state, selector(script(state).Value));
                };

    public static SelectrOf<TResult> SelectMany<TSource, TResult>(
        this SelectrOf<TSource> source,
        Func<TSource, SelectrOf<TResult>> selector) =>
            state =>
                {
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TResult>(state);
                    var result = source(state);
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TResult>(state);
                    return selector(result.Value)(state);
                };

    public static SelectrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, SelectrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
            => script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}