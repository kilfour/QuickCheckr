namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Function<T>(Func<T, IEnumerable<T>> function) =>
        OnType<T>(s => s.Classify(Try.Function(function)));
}


