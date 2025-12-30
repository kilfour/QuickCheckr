using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

namespace QuickCheckr;

public static class Shrink
{
    public static ShrinkOption<T> Fuzzr<T>(FuzzrOf<T> fuzzr, int times = 10)
        => new ShrinkOptionFuzzr<T>(fuzzr, times);
    public static ShrinkOption<T> Values<T>(params T[] values)
        => new ShrinkOptionFunction<T>(_ => values);
    public static ShrinkOption<T> Function<T>(Func<T, IEnumerable<T>> function)
        => new ShrinkOptionFunction<T>(function);
    public static ShrinkOption<T> Default<T>() => new ShrinkOptionDefault<T>();
}
