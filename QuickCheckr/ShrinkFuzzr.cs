using QuickFuzzr;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Fuzzr<T>(FuzzrOf<T> fuzzr, int times = 10) =>
        OnType<T>(s => s.Classify(Try.Fuzzr(fuzzr, times)));
}


