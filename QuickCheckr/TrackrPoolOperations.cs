using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public class TrackrPoolOperations<T>(CheckrState state, int id)
{
    private readonly int id = id;
    private readonly CheckrState state = state;

    public TrackrPoolOperations<T> Update(T element)
    {
        state.Memory.GetPoolDictionary<T>()[id] = element;
        return this;
    }

    public TrackrPoolOperations<T> Remove()
    {
        state.Memory.GetPoolDictionary<T>().Remove(id);
        return this;
    }
}
