using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Keep<T>()
        => OnType<T>(s => s.Classify(new ClassificationOptionKeep<T>()));
}