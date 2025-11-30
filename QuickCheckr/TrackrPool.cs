using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<IEnumerable<T>> PoolFor<T>() =>
        state => CheckrResult.Some(state, state.Memory.CreatePool<T>());

    public static CheckrOf<IEnumerable<T>> Pool<T>() =>
        state => CheckrResult.Some(state, state.Memory.GetPool<T>());

    public static CheckrOf<T> Pool<T>(string key, T value) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key, () => Fuzzr.Int(0, int.MaxValue)(state.FuzzrState).Value);
            return CheckrResult.Some(state, state.Memory.StoreInPool(id, value));
        };

    public static CheckrOf<T?> OneOfPool<T>(string key) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(state.Memory.GetPoolIds<T>()).WithDefault(-1)(state.FuzzrState).Value);
            if (id == -1) return CheckrResult.Default<T?>(state);
            return CheckrResult.Some(state, state.Memory.GetFromPool<T>(id))!;
        };
}