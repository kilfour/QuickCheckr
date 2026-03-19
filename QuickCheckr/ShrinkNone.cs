using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker None<T>()
        => OnType<T>(s => s.Classify(new ClassificationOptionNone<T>()));
}