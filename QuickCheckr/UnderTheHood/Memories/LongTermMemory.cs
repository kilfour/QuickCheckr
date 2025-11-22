namespace QuickCheckr.UnderTheHood.Memories;

public class LongTermMemory
{
    private MemoryBank shashed = new();

    public T StoreStashed<T>(string key, Func<T> factory)
        => shashed.Store(key, factory)!;

    public void Reset() => shashed = new();
}
