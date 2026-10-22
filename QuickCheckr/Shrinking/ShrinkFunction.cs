using QuickCheckr.Shrinking;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Function<T>(Func<T, IEnumerable<T>> function) =>
        Shrinkr.For<T>().Classify(Try.Function(function));
}


