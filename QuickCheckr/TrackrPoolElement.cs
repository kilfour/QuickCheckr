using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>
/// Represents an element selected from a tracked pool and provides operations that update that element.
/// </summary>
public sealed class PoolElement<T>
{
    private readonly int id;
    private readonly T? value;

    internal PoolElement(int id, T? value)
    {
        this.id = id;
        this.value = value;
    }

    public int Id => id;
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
