using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Adds a value to a pool under a generated identifier and returns the stored value.  
    /// Use to register objects that should participate in later pool-based selections.
    /// </summary>
    public static CheckrOf<T> ToPool<T>(string key, T value) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.Counter(typeof(T).Name + key)(state.FuzzrState).Value);
            state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}-{id}");
            return CheckrResult.Some(state, state.Memory.StoreInPool(id, key, value));
        };
}
