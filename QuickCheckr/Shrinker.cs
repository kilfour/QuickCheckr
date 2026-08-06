using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public record Shrinker(
    Func<ShrinkingInfo, bool> Condition,
    IClassificationOption[] Classifiers,
    ICollapseOption[] Collapsers,
    IReducerOption[] Reducers);
