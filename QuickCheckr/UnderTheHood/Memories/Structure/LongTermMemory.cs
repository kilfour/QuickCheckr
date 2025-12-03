namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class LongTermMemory
{
    public void Reset()
    {
        stashed = new();
        pools = new();
        codeArrange = new();
    }

    // --------------------------------------------------------------------------
    // Stashed
    private MemoryBank stashed = new();
    public T StoreStashed<T>(string key, Func<T> factory)
        => stashed.Store(key, factory)!;
    public T GetStashed<T>(string key) => stashed.Get<T>(key)!;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Pool
    private MemoryBank pools = new();
    public IEnumerable<T> GetPool<T>()
        => pools.Get<Dictionary<int, T>>(typeof(T).FullName!).Values;

    public IEnumerable<T> CreatePool<T>()
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        if (pool is null)
        {
            pool = [];
            pools.Store(typeof(T).FullName!, () => pool);
        }
        return pool.Values;
    }

    public IEnumerable<int> GetPoolIds<T>()
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        if (pool is null)
        {
            pool = [];
            pools.Store(typeof(T).FullName!, () => pool);
        }
        return pool.Keys;
    }

    public T? GetFromPool<T>(int id)
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        if (pool is null)
        {
            pool = [];
            pools.Store(typeof(T).FullName!, () => pool);
        }
        if (!pool.TryGetValue(id, out T? value))
        {
            return default;
        }
        return pool[id];
    }

    public T StoreInPool<T>(int id, T value)
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        pool[id] = value;
        return value;
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Code Arrange
    // --------------------------------------------------------------------------
    private MemoryBank codeArrange = new();
    public void StoreCodeArrange(string key, Func<string> codeFactory)
        => codeArrange.Store(key, codeFactory);
    public IReadOnlyList<object?> GetCodeArrangeValues()
        => [.. codeArrange.Values.Values];
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Code Assert
    // --------------------------------------------------------------------------
    private MemoryBank codeAssert = new();
    public void StoreCodeAssert(string key, Func<string> codeFactory)
        => codeAssert.Store(key, codeFactory);
    public IReadOnlyList<object?> GetCodeAssertValues()
        => [.. codeAssert.Values.Values];
    // --------------------------------------------------------------------------
}
