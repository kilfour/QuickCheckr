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
            state, (() => predicate(state.Memory.GetPool<T>()), OneOfPool<T>(key, checkr)));
}