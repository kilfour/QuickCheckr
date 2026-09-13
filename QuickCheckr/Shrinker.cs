using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

/// <summary>
/// Represents a shrinking strategy created by the <see cref="Shrink"/> helpers.
/// </summary>
public sealed class Shrinker
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
