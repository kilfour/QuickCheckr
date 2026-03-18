using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.ShrinkerBuilders;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker None<T>()
        => OnType<T>(s => s.Classify(new ClassificationOptionNone<T>()));
}