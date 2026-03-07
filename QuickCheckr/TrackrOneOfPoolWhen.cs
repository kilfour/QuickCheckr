using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> OneOfPoolWhen<T>(
        string key,
        Func<T, bool> predicate,
        Func<PoolElement<T>, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds(predicate))(state.FuzzrState).Value);
            state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}-{id}");
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result)
            {
                return CheckrResult.Some(state, checkr(new PoolElement<T>(id, value))(state).Value);
            }
            return CheckrResult.CaseOnly(state);
        };


    public static CheckrOf<Case> OneOfPoolWhen<T1, T2>(
        string key,
        Func<T1, T2, bool> predicate,
        Func<PoolElement<T1>, PoolElement<T2>, CheckrOf<Case>> checkr) =>
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
            state.Memory.StorePoolTrace(key, () => $"( {typeof(T1).Name}-{id1}, {typeof(T2).Name}-{id2} )");
            var (result1, value1) = state.Memory.GetFromPool<T1>(id1);
            var (result2, value2) = state.Memory.GetFromPool<T2>(id2);
            if (result1 && result2) return CheckrResult.Some(state, checkr(new PoolElement<T1>(id1, value1), new PoolElement<T2>(id2, value2))(state).Value);
            return CheckrResult.CaseOnly(state);
        };
}
