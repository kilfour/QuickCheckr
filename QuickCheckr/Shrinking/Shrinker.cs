using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr.Shrinking;

/// <summary>
/// Represents a shrinking strategy created by the <see cref="Shrink"/> helpers.
/// </summary>
public class Shrinker
{
    internal Shrinker(
        Func<ShrinkingInfo, bool> condition,
        IClassificationOption[] classifiers,
        ITraversalOption[] traversals,
        IReducerOption[] reducers)
    {
        Condition = condition;
        Classifiers = classifiers;
        Traversals = traversals;
        Reducers = reducers;
    }

    internal Func<ShrinkingInfo, bool> Condition { get; }
    internal IClassificationOption[] Classifiers { get; }
    internal ITraversalOption[] Traversals { get; }
    internal IReducerOption[] Reducers { get; }
    internal ICollapseOption[] Collapsers { get; init; } = [];
}

/// <summary>
/// An immutable shrinking strategy that can be used directly wherever a Shrinker is accepted.
/// Repeated configuration calls append options in declaration order; conditions are combined with AND.
/// </summary>
public sealed class Shrinker<T> : Shrinker
{
    internal Shrinker() : base(info => typeof(T).IsAssignableFrom(info.ActualType), [], [], []) { }

    private Shrinker(Func<ShrinkingInfo, bool> condition,
        IClassificationOption[] classifiers, ITraversalOption[] traversals, IReducerOption[] reducers)
        : base(condition, classifiers, traversals, reducers) { }

    /// <summary>Restricts this strategy to values matching the predicate.</summary>
    public Shrinker<T> When(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Copy(info => Condition(info) && predicate((T)info.Value!));
    }

    /// <summary>Restricts this strategy using value and member-path conditions.</summary>
    public Shrinker<T> When(Action<IShrinkerConditionBuilder<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new ShrinkerConditionBuilder<T>();
        configure(builder);
        var condition = builder.GetCondition();
        return Copy(info => Condition(info) && condition(info));
    }

    /// <summary>Appends classification strategies.</summary>
    public Shrinker<T> Classify(params ClassificationOption<T>[] classifiers)
    {
        ArgumentNullException.ThrowIfNull(classifiers);
        return Copy(classifiers: [.. Classifiers, .. classifiers]);
    }

    /// <summary>Appends reducer factories, preserving a fresh reducer for each reduction.</summary>
    public Shrinker<T> Reduce(params Func<Reducer<T>>[] reducers)
    {
        ArgumentNullException.ThrowIfNull(reducers);
        return Copy(reducers: [.. Reducers, .. reducers.Select(factory => new TypeReducerOption<T>(factory))]);
    }

    /// <summary>Appends member traversal and reconstruction strategies.</summary>
    public Shrinker<T> Traverse(params TraversalOption<T>[] traversals)
    {
        ArgumentNullException.ThrowIfNull(traversals);
        return Copy(traversals: [.. Traversals, .. traversals]);
    }

    internal Shrinker<T> AppendCollapsers(ICollapseOption[] collapsers) =>
        Copy(collapsers: [.. Collapsers, .. collapsers]);

    private Shrinker<T> Copy(Func<ShrinkingInfo, bool>? condition = null,
        IClassificationOption[]? classifiers = null, ITraversalOption[]? traversals = null,
        IReducerOption[]? reducers = null, ICollapseOption[]? collapsers = null) =>
        new(condition ?? Condition, classifiers ?? Classifiers, traversals ?? Traversals, reducers ?? Reducers)
        { Collapsers = collapsers ?? Collapsers };
}
