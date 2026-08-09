using QuickFuzzr;

namespace QuickCheckr.Supplier;

public static class Supplr_ToLinq
{
    public static SupplrOf<TValueTwo> Select<TValueOne, TValueTwo>(
        this SupplrOf<TValueOne> source,
        Func<TValueOne, TValueTwo> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return state =>
        {
            var result = source(state);
            var definition = new DerivedDefinition<TValueTwo>(
                () => result.Definition.GetFuzzr().Select(selector),
                result.State.GetShrinkers);
            return SupplrResult.Some(result.State, definition);
        };
    }

    public static SupplrOf<TResult> SelectMany<TSource, TResult>(
        this SupplrOf<TSource> source,
        Func<TSource, SupplrOf<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return state =>
        {
            var result = source(state);
            var definition = new DerivedDefinition<TResult>(() =>
                result.Definition.GetFuzzr().SelectMany(value =>
                    selector(value)(result.State).Definition.GetFuzzr()),
                result.State.GetShrinkers);
            return SupplrResult.Some(result.State, definition);
        };
    }

    public static SupplrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this SupplrOf<TValueOne> source,
        Func<TValueOne, SupplrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(projector);

        return state =>
        {
            var result = source(state);
            var definition = new DerivedDefinition<TValueThree>(() =>
                result.Definition.GetFuzzr().SelectMany(
                    value => selector(value)(result.State).Definition.GetFuzzr(),
                    projector),
                result.State.GetShrinkers);
            return SupplrResult.Some(result.State, definition);
        };
    }
}
