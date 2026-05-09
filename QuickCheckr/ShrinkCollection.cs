using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Collection<T>(params IEnumerable<T>[] values) =>
        OnType<IEnumerable<T>>(s => s.Classify(Try.Collection(values)));
}


