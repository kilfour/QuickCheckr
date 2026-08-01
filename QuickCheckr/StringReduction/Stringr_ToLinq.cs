using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static class Selectr_ToLinq
{
    public static SelectrOf<TValueTwo> Select<TValueOne, TValueTwo>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, TValueTwo> selector) =>
            state =>
            {
                var result = script(state);
                if (!result.Succeeded)
                    return SelectrResult.Default<TValueTwo>(state);

                return SelectrResult.Some(state, selector(result.Value));
            };

    public static SelectrOf<TResult> SelectMany<TSource, TResult>(
        this SelectrOf<TSource> source,
        Func<TSource, SelectrOf<TResult>> selector) =>
            state =>
            {
                var result = source(state);
                if (!result.Succeeded)
                    return SelectrResult.Default<TResult>(state);

                return selector(result.Value)(state);
            };

    public static SelectrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, SelectrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
            => script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}
