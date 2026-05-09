using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Values<T>(params T[] values) =>
        OnType<T>(s => s.Classify(Try.Values(values)));
}


