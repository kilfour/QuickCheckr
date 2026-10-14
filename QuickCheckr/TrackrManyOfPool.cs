using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> ManyOfPool<T>(
        string key,
        int numberOfItems,
        Func<IReadOnlyList<PoolElement<T>>, CheckrOf<Case>> checkr) =>
        async state =>
        {
            var ids = state.Memory.StoreVariable(key,
                () =>
                {
                    var poolIds = state.Memory.GetPoolIds<T>();
                    if (!poolIds.Any())
                        throw new EmptyPoolException(key, typeof(T), "ManyOfPool<T>");
                    return Fuzzr.OneOf(poolIds).Many(numberOfItems)(state.FuzzrState).Value;
                });
            state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}: [ {string.Join(", ", ids!)} ]");
            var list = new List<PoolElement<T>>();
            foreach (var id in ids!)
            {
                var (result, value) = state.Memory.GetFromPool<T>(id);
                if (!result)
                    return CheckrResult.CaseOnly(state);
                list.Add(new PoolElement<T>(id, value!));
            }
            return await checkr(list)(state).ConfigureAwait(false);
        };
}
