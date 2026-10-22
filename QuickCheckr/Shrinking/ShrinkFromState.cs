using QuickCheckr.Shrinking;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker ValueFromState<T>(T current, T redux) =>
        Shrinkr.For<T>()
            .Classify(Try.Default<T>(), Try.Values(current))
            .Reduce(Reduce.Values([redux, current]));

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState([.. current], redux);
}


