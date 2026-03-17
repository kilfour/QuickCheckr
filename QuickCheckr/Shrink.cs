using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr;

public interface IShrinkerCondition<T>
{
    ShrinkerConditionBuilder<T> Value(Func<T, bool> predicate);
}

public class ShrinkerConditionBuilder<T> : IShrinkerCondition<T>
{
    public List<Func<ShrinkingInfo, bool>> predicates = [];

    public ShrinkerConditionBuilder<T> Value(Func<T, bool> predicate)
    {
        predicates.Add(a => predicate((T)a.Value!));
        return this;
    }

    public Func<ShrinkingInfo, bool> GetCondition()
    {
        if (predicates.Count == 0) return a => typeof(T).IsAssignableFrom(a.ActualType);
        return a => predicates.Prepend(a => typeof(T).IsAssignableFrom(a.ActualType)).All(p => p(a));
    }
}

public static class Shrink
{
    public static Shrinker When<T>(
        Func<ShrinkerConditionBuilder<T>, ShrinkerConditionBuilder<T>> condition,
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(condition(new ShrinkerConditionBuilder<T>()).GetCondition(),
            classifiersFactory(new ClassificationBuilder<T>()),
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker When<T>(
        Func<ShrinkerConditionBuilder<T>, ShrinkerConditionBuilder<T>> condition,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => new(condition(new ShrinkerConditionBuilder<T>()).GetCondition(),
            [],
            [.. reducersFactory(new ReducerBuilder<T>()).Select(a => new TypeReducerOption<T>(a))]);

    public static Shrinker When<T>(
        Func<ShrinkerConditionBuilder<T>, ShrinkerConditionBuilder<T>> condition,
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory)
        => new(condition(new ShrinkerConditionBuilder<T>()).GetCondition(),
            classifiersFactory(new ClassificationBuilder<T>()),
            []);

    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory,
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => When(s => s, classifiersFactory, reducersFactory);

    public static Shrinker OnType<T>(
        Func<ClassificationBuilder<T>, ClassificationOption<T>[]> classifiersFactory)
        => When(s => s, classifiersFactory);

    public static Shrinker OnType<T>(
        Func<ReducerBuilder<T>, Func<Reducer<T>>[]> reducersFactory)
        => When(s => s, reducersFactory);

    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType<T>(
            s => s.Classify(Try.Default<T>(), Try.Values(current)),
            s => s.Simplify(Reduce.Values([redux, current])));

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}


