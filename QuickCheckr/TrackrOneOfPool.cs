using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
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
        async state =>
        {
            var id = state.Memory.StoreVariable(key,
                () =>
                {
                    var poolIds = state.Memory.GetPoolIds<T>();
                    if (!poolIds.Any())
                        throw new EmptyPoolException(key, typeof(T), "OneOfPool<T>");
                    return Fuzzr.OneOf(poolIds)(state.FuzzrState).Value;
                });
            state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}-{id}");
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result)
            {
                return await checkr(new PoolElement<T>(id, value!))(state).ConfigureAwait(false);
            }
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> OneOfPool<T1, T2>(
        string key,
        Func<T1, T2, CheckrOf<Case>> checkr) =>
        async state =>
        {
            var id1 = state.Memory.StoreVariable($"{key}-1",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T1>(_ => true))(state.FuzzrState).Value);
            var (result1, value1) = state.Memory.GetFromPool<T1>(id1);

            var id2 = state.Memory.StoreVariable($"{key}-2",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T2>(_ => true))(state.FuzzrState).Value);
            var (result2, value2) = state.Memory.GetFromPool<T2>(id2);

            if (result1 && result2) return await checkr(value1!, value2!)(state).ConfigureAwait(false);
            return CheckrResult.CaseOnly(state);
        };
}
