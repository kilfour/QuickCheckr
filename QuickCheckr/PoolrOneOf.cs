using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Poolr
{
    public static CheckrOf<T> OneOf<T>(string key) =>
        OneOfElement<T>(key).Select(element => element.Value);

    private static CheckrOf<PoolElement<T>> OneOfElement<T>(string key, Func<T, bool>? predicate = null) =>
        state =>
        {
            var id = state.Memory.StoreVariable(key,
                () =>
                {
                    var poolIds = predicate is null
                        ? state.Memory.GetPoolIds<T>()
                        : state.Memory.GetPoolIds(predicate);
                    if (!poolIds.Any())
                    {
                        if (predicate is not null)
                            throw new NoMatchingPoolElementException(key, typeof(T));
                        throw new EmptyPoolException(key, typeof(T), "Poolr.OneOf<T>");
                    }
                    return Fuzzr.OneOf(poolIds)(state.FuzzrState).Value;
                });
            state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}-{id}");
            var (result, value) = state.Memory.GetFromPool<T>(id);
            if (result && (predicate is null || predicate(value!)))
            {
                return CheckrResult.Some<PoolElement<T>>(state, new(id, value!));
            }
            state.AbortExecution();
            return CheckrResult.Default<PoolElement<T>>(state);
        };
}
