using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public sealed class PoolEntryOperations<T>(int id)
{
    public CheckrOf<Case> Update(T replacement) =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>()[id] = replacement;
            return CheckrResult.CaseOnly(state);
        };

    public CheckrOf<Case> Remove() =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>().Remove(id);
            return CheckrResult.CaseOnly(state);
        };
}