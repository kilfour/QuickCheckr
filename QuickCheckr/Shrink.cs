using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static class Shrink
{

    public static Shrinker OnType<T>(Func<ClassificationOption<T>> classifier)
        => new(() => [classifier()], []);

    public static Shrinker OnType<T>(Func<ClassificationOption<T>[]> classifiers)
        => new(classifiers, []);

    public static Shrinker OnType<T>(Func<Reducer<T>> reducer)
        => new(() => [], [new TypeReducerOption<T>(reducer)]);

    public static Shrinker OnType<T>(ClassificationOption<T>[] classifiers, Func<Reducer<T>>[] reducers)
        => new(() => classifiers, [.. reducers.Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType([Try.Default<T>(), Try.Values(current)], [Reduce.Values([redux, current])]);

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}
