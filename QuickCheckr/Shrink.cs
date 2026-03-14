using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static class Shrink
{
    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(() =>
            classifiersFactory(new ClassificationBuilder<T>()),
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    // public static Shrinker OnType<T>(ClassificationOption<T> classifier)
    //     => new(() => [classifier], []);

    public static Shrinker OnType<T>(params ClassificationOption<T>[] classifiers)
        => new(() => classifiers, []);

    public static Shrinker OnType<T>(params Func<Reducer<T>>[] reducers)
        => new(() => [], [.. reducers.Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker OnType<T>(ClassificationOption<T>[] classifiers, Func<Reducer<T>>[] reducers)
        => new(() => classifiers, [.. reducers.Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType([Try.Default<T>(), Try.Values(current)], [Reduce.Values([redux, current])]);

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}

public class ClassificationBuilder<T>
{
    public ClassificationOption<T>[] Classify(params ClassificationOption<T>[] classifiers)
        => classifiers;
}

public class ReducerBuilder<T>
{
    public Func<Reducer<T>>[] Simplify(params Func<Reducer<T>>[] reducers)
        => reducers;
}
