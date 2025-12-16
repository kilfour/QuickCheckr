using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Creates a new pool for values of the given type and returns it.  
    /// Use when you want to start collecting objects across executions within a run.
    /// </summary>
    public static CheckrOf<IEnumerable<T>> PoolFor<T>() =>
        state => CheckrResult.Some(state, state.Memory.CreatePool<T>());

    /// <summary>
    /// Retrieves the existing pool for values of the given type.  
    /// Use to observe or draw from objects accumulated earlier in the run.
    /// </summary>
    public static CheckrOf<IEnumerable<T>> Pool<T>() =>
        state => CheckrResult.Some(state, state.Memory.GetPool<T>());

    /// <summary>
    /// Adds a value to a pool under a generated identifier and returns the stored value.  
    /// Use to register objects that should participate in later pool-based selections.
    /// </summary>
    public static CheckrOf<T> Pool<T>(string key, T value) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key, () => Fuzzr.Int(0, int.MaxValue).Unique("Quick-Checkr-Pool-Id")(state.FuzzrState).Value);
            return CheckrResult.Some(state, state.Memory.StoreInPool(id, value));
        };

    /// <summary>
    /// Selects a value at random from a pool, or returns a default value (typically null) when the pool is empty.  
    /// Use when modelling behaviour that may or may not have stored items available during the run.
    /// </summary>
    public static CheckrOf<Case> OneOfPool<T>(string key, Func<T, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T>())(state.FuzzrState).Value);
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result) return CheckrResult.Some(state, checkr(value!)(state).Value);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> OneOfPool<T1, T2>(string key, Func<T1, T2, CheckrOf<Case>> checkr) =>
        state =>
        {
            var id1 = state.Memory.StoreVariable($"{key}-1",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T1>())(state.FuzzrState).Value);
            var (result1, value1) = state.Memory.GetFromPool<T1>(id1);

            var id2 = state.Memory.StoreVariable($"{key}-2",
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T2>())(state.FuzzrState).Value);
            var (result2, value2) = state.Memory.GetFromPool<T2>(id2);

            if (result1 && result2) return CheckrResult.Some(state, checkr(value1!, value2!)(state).Value);
            return CheckrResult.CaseOnly(state);
        };


    public static CheckrOf<Case> ClearPool<T>() =>
        state =>
        {
            state.Memory.ClearPool<T>();
            return CheckrResult.Default<Case>(state);
        };
}