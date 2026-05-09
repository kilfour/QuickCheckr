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

    private static Func<ShrinkingInfo, bool> GetCondition<T>(
        Action<IShrinkerConditionBuilder<T>> condition)
    {
        var builder = new ShrinkerConditionBuilder<T>();
        condition(builder);
        return builder.GetCondition();
    }
}


