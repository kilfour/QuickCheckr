using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker None<T>()
        => OnType<T>(s => s.Classify(new ClassificationOptionNone<T>()));
}