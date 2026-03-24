using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public static class DefaultReducersRegistry
{
    public static readonly Dictionary<Type, Func<Func<ReductionResult, Reduction>>> ReducerFactories =
        new()
        {
            {typeof(int), () => Convert(Reduce.TowardsZero)},
            {typeof(short), () => Convert(Reduce.ShortTowardsZero)}
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