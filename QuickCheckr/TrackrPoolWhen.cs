using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<CheckrOption> PoolWhen<T>(
        Func<IReadOnlyCollection<T>, bool> predicate,
        CheckrOf<Case> checkr) =>
        state => CheckrResult.Some<CheckrOption>(
            state, new(() => predicate((IReadOnlyCollection<T>)state.Memory.GetPool<T>()), checkr));

    public static CheckrOf<CheckrOption> PoolWhen<T>(
        string key,
        Func<IReadOnlyCollection<T>, bool> predicate,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => CheckrResult.Some<CheckrOption>(
            state, new(() => predicate((IReadOnlyCollection<T>)state.Memory.GetPool<T>()), OneOfPool(key, checkr)));

    public static CheckrOf<CheckrOption> PoolWhen<T>(
        string key,
        Func<T, bool> predicate,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => CheckrResult.Some<CheckrOption>(
            state, new(() => state.Memory.GetPool<T>().Any(predicate), OneOfPoolWhen(key, predicate, checkr)));

    public static CheckrOf<CheckrOption> PoolWhen<T1, T2>(
        string key,
        Func<T1, T2, bool> predicate,
        Func<PoolElement<T1>, PoolElement<T2>, CheckrOf<Case>> checkr) =>
        state =>
            CheckrResult.Some<CheckrOption>(
                state,
                new(
                    () => HasAtleastOne(predicate, state.Memory.GetPool<T1>(), state.Memory.GetPool<T2>()),
                    OneOfPoolWhen(key, predicate, checkr)));

    private static bool HasAtleastOne<T1, T2>(Func<T1, T2, bool> predicate, IEnumerable<T1> pool1, IEnumerable<T2> pool2)
        => pool1.SelectMany(t1 => pool2.Select(t2 => predicate(t1, t2))).Any(a => a);
}