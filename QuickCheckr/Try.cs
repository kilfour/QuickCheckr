using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickFuzzr;

namespace QuickCheckr;

public static class Try
{
    public static ClassificationOption<T> Fuzzr<T>(FuzzrOf<T> fuzzr, int times = 10)
        => new ClassificationOptionFuzzr<T>(fuzzr, times);

    public static ClassificationOption<T> Values<T>(params T[] values)
        => new ClassificationOptionFunction<T>(_ => values);

    public static ClassificationOption<IEnumerable<T>> Collection<T>(params IEnumerable<T>[] values)
        => new ClassificationOptionFunction<IEnumerable<T>>(_ => values);

    public static ClassificationOption<T> Function<T>(Func<T, IEnumerable<T>> function)
        => new ClassificationOptionFunction<T>(function);

    public static ClassificationOption<T> Default<T>() => new ClassificationOptionDefault<T>();

    public static ClassificationOption<T> None<T>() => new ClassificationOptionNone<T>();

    public static ClassificationOption<T> Keep<T>() => new ClassificationOptionKeep<T>();
}

