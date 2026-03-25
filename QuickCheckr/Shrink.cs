using QuickCheckr;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker When<T>(
        Action<IShrinkerConditionBuilder<T>> condition,
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(GetCondition(condition),
            classifiersFactory(new ClassificationBuilder<T>()),
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    private static Func<ShrinkingInfo, bool> GetCondition<T>(
        Action<IShrinkerConditionBuilder<T>> condition)
    {
        var builder = new ShrinkerConditionBuilder<T>();
        condition(builder);
        return builder.GetCondition();
    }

    public static Shrinker When<T>(
        Action<IShrinkerConditionBuilder<T>> condition,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(GetCondition(condition),
            [],
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker When<T>(
        Action<IShrinkerConditionBuilder<T>> condition,
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory)
        => new(GetCondition(condition),
            classifiersFactory(new ClassificationBuilder<T>()),
            []);

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

    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType<T>(
            s => s.Classify(Try.Default<T>(), Try.Values(current)),
            s => s.Simplify(Reduce.Values([redux, current])));

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}


