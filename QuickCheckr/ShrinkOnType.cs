using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => When(s => { }, classifiersFactory, reducersFactory);

    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory)
        => When(s => { }, classifiersFactory);

    public static Shrinker OnType<T>(
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => When(s => { }, reducersFactory);
}


