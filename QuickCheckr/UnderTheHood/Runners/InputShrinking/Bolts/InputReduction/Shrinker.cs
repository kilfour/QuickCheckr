using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public record Shrinker(Func<IClassificationOption[]> Classifiers, IReducerOption[] Reducers);