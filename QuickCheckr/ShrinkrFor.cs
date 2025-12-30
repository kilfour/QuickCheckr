using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Shrinkr
{
    public static CheckrOf<Case> For<T>(params T[] values)
        => For<T>([Shrink.Values(values)]);

    public static CheckrOf<Case> For<T>(Func<T, IEnumerable<T>> function)
        => For<T>([Shrink.Function(function)]);

    public static CheckrOf<Case> For<T>(FuzzrOf<T> fuzzr, int times = 10)
        => For<T>([Shrink.Fuzzr(fuzzr, times)]);

    public static CheckrOf<Case> For<T>(params ShrinkOption<T>[] options)
        => state =>
        {
            state.Memory.StoreShrinkerForType<T>(options);
            return CheckrResult.CaseOnly(state);
        };
}
