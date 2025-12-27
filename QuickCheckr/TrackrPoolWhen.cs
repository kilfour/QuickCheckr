using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> PoolWhen<T>(
        Func<IEnumerable<T>, bool> predicate,
        CheckrOf<Case> checkr) =>
        state => CheckrResult.Some<(Func<bool> condition, CheckrOf<Case> checkr)>(
            state, (() => predicate(state.Memory.GetPool<T>()), checkr));

    public static CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> PoolWhen<T>(
        Func<IEnumerable<T>, bool> predicate,
        string key,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state => CheckrResult.Some<(Func<bool> condition, CheckrOf<Case> checkr)>(
            state, (() => predicate(state.Memory.GetPool<T>()), OneOfPool(key, checkr)));


    public static CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> PoolWhen<T1, T2>(
        string key,
        Func<T1, T2, bool> predicate,
        Func<T1, T2, CheckrOf<Case>> checkr) =>
        state =>
            CheckrResult.Some<(Func<bool> condition, CheckrOf<Case> checkr)>(
                state,
                (() =>
                    HasAtleastOne(predicate, state.Memory.GetPool<T1>(), state.Memory.GetPool<T2>()),
                    OneOfPoolWhen(key, predicate, checkr)));

    private static bool HasAtleastOne<T1, T2>(Func<T1, T2, bool> predicate, IEnumerable<T1> pool1, IEnumerable<T2> pool2)
        => pool1.SelectMany(t1 => pool2.Select(t2 => predicate(t1, t2))).All(a => a);
}