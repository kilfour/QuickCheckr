using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

/// <summary>
/// Provides LINQ query composition for Stringr selectors.
/// </summary>
public static class Selectr_ToLinq
{
    /// <summary>
    /// Projects the value produced by a successful selector.
    /// A failed selector is propagated without invoking the projection.
    /// </summary>
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

    /// <summary>
    /// Continues a successful selector with another selector derived from its value.
    /// Use to compose dependent steps with LINQ method syntax.
    /// </summary>
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

    /// <summary>
    /// Combines two dependent selectors and projects their values.
    /// This overload enables C# LINQ query syntax for Stringr scripts.
    /// </summary>
    public static SelectrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, SelectrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
            => script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}
