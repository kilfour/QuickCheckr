namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker ValueFromState<T>(T current, T redux) =>
        OnType<T>(
            s => s.Classify(Try.Default<T>(), Try.Values(current)),
            s => s.Simplify(Reduce.Values([redux, current])));

    public static Shrinker CollectionFromState<T>(IEnumerable<T> current, IEnumerable<T> redux) =>
        ValueFromState(current, redux);
}


