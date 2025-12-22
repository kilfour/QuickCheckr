using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public class PoolElement<T>(int id, T? value)
{
    private readonly int id = id;

    public T Value => value!;

    public CheckrOf<Case> Replace(T element) =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>()[id] = element;
            return CheckrResult.CaseOnly(state);
        };

    public CheckrOf<Case> Remove() =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>().Remove(id);
            return CheckrResult.CaseOnly(state);
        };
}
