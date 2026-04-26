using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolForEach<T>(
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => Combine.Checkrs(
            state.Memory.GetPoolDictionary<T>()
                .ToList()
                .Select(a => ApplyCheckrToPoolElement(a, checkr)))
            (state);

    public static CheckrOf<Case> PoolForEachWhen<T>(
        Func<T, bool> condition,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => Combine.Checkrs(
            state.Memory.GetPoolDictionary<T>()
                .Where(a => condition(a.Value))
                .ToList()
                .Select(a => ApplyCheckrToPoolElement(a, checkr)))
            (state);

    private static CheckrOf<Case> ApplyCheckrToPoolElement<T>(
        KeyValuePair<int, T> kv,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => checkr(new PoolElement<T>(kv.Key, kv.Value))(state);
}