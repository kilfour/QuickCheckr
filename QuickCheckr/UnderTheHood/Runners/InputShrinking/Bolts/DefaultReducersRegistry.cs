using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

internal static class DefaultReducersRegistry
{
    public static readonly Dictionary<Type, Func<Func<ReductionResult, Reduction>>> ReducerFactories =
        new()
        {
            {typeof(int), () => Convert(Reduce.TowardsZero)},
            {typeof(short), () => Convert(Reduce.ShortTowardsZero)},
            {typeof(decimal), () => Convert(Reduce.Towards(0m, 2))},
            {typeof(double), () => Convert(Reduce.Towards(0d, 2))},
            {typeof(string), () => Convert(Reduce.MinimizeString)}
        };

    public static Func<Func<ReductionResult, Reduction>>? TryGetReducerFactory(Type type)
    {
        if (ReducerFactories.TryGetValue(type, out var factory))
            return factory;
        return null;
    }

    private static Func<ReductionResult, Reduction> Convert<T>(Func<Reducer<T>> reducer)
    {
        var f = reducer();
        return a => f(a);
    }
}
