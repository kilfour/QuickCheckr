using QuickCheckr.Shrinking;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker None<T>()
        => Shrinkr.For<T>().Classify(new ClassificationOptionNone<T>());
}