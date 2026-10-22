using QuickCheckr.Shrinking;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Collection<T>(params IEnumerable<T>[] values) =>
        Shrinkr.For<IEnumerable<T>>().Classify(Try.Collection(values));
}


