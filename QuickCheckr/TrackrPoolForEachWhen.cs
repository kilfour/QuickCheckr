using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolForEachWhen<T>(
        Func<T, bool> condition,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => Combine.Checkrs(
            state.Memory.GetPoolDictionary<T>()
                .Where(a => condition(a.Value))
                .ToList()
                .Select(a => ApplyCheckrToPoolElement(a, checkr)))
            (state);
}