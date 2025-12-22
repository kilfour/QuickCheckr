using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Selects a value at random from a pool, or returns a default value (typically null) when the pool is empty.  
    /// Use when modelling behaviour that may or may not have stored items available during the run.
    /// </summary>
    public static CheckrOf<Case> OneOfPool<T>(
        string key,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T>())(state.FuzzrState).Value);
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result)
            {
                return CheckrResult.Some(state, checkr(new PoolElement<T>(id, value!))(state).Value);
            }
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> OneOfPool<T>(
        string key,
        Func<T, bool> predicate,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds(predicate))(state.FuzzrState).Value);
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result)
            {
                return CheckrResult.Some(state, checkr(new PoolElement<T>(id, value))(state).Value);
            }
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> OneOfPool<T1, T2>(
        string key,
        Func<T1, T2, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id1 = state.Memory.StoreVariable($"{key}-1",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T1>(_ => true))(state.FuzzrState).Value);
            var (result1, value1) = state.Memory.GetFromPool<T1>(id1);

            var id2 = state.Memory.StoreVariable($"{key}-2",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T2>(_ => true))(state.FuzzrState).Value);
            var (result2, value2) = state.Memory.GetFromPool<T2>(id2);

            if (result1 && result2) return CheckrResult.Some(state, checkr(value1!, value2!)(state).Value);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> OneOfPool<T1, T2>(
        string key,
        Func<T1, T2, bool> predicate,
        Func<T1, T2, CheckrOf<Case>> checkr) =>
        state =>
        {
            var pool1 = state.Memory.GetPoolDictionary<T1>();
            var pool2 = state.Memory.GetPoolDictionary<T2>();
            var combined =
                from t1 in pool1
                from t2 in pool2
                where predicate(t1.Value, t2.Value)
                select (t1.Key, t2.Key);
            var (id1, id2) = state.Memory.StoreVariable($"{key}",
                () => Fuzzr.OneOf(combined).WithDefault((-1, -1))(state.FuzzrState).Value);
            var (result1, value1) = state.Memory.GetFromPool<T1>(id1);
            var (result2, value2) = state.Memory.GetFromPool<T2>(id2);
            if (result1 && result2) return CheckrResult.Some(state, checkr(value1!, value2!)(state).Value);
            return CheckrResult.CaseOnly(state);
        };
}
