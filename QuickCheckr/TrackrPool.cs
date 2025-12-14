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
    public static CheckrOf<T?> OneOfPool<T>(string key) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T>()).WithDefault(-1)(state.FuzzrState).Value);
            if (id == -1) return CheckrResult.Default<T?>(state);
            return CheckrResult.Some(state, state.Memory.GetFromPool<T>(id))!;
        };

    public static CheckrOf<Case> ClearPool<T>() =>
        state =>
        {
            state.Memory.ClearPool<T>();
            return CheckrResult.Default<Case>(state);
        };
}