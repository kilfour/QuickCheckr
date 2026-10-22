using QuickCheckr.Shrinking;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Values<T>(params T[] values) =>
        Shrinkr.For<T>().Classify(Try.Values(values));
}


