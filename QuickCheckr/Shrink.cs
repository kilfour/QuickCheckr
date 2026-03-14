using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr;

public static class Shrink
{
    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(() =>
            classifiersFactory(new ClassificationBuilder<T>()),
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory)
        => new(() =>
            classifiersFactory(new ClassificationBuilder<T>()),
            []);

    public static Shrinker OnType<T>(
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(() =>
            [],
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType<T>(
            s => s.Classify(Try.Default<T>(), Try.Values(current)),
            s => s.Simplify(Reduce.Values([redux, current])));

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}


