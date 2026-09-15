namespace QuickCheckr.UnderTheHood.Memories.Structure;

internal sealed class LongTermMemory
{
    public void Reset()
    {
        stashed = new();
        pools = new();
    }

    public void DisposeStash()
        => DisposeStashAsync().GetAwaiter().GetResult();

    public async ValueTask DisposeStashAsync()
    {
        List<Exception>? errors = null;
        foreach (var item in stashed.Values.Values.ToArray())
        {
            try
            {
                if (item is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (item is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception exception) { (errors ??= []).Add(exception); }
        }
        if (errors is { Count: 1 })
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(errors[0]).Throw();
        if (errors is { Count: > 1 })
            throw new AggregateException(errors);
    }

    // --------------------------------------------------------------------------
    // Variables
    private readonly Dictionary<string, object> variables = [];
    public void StoreLongTermVariable(string key, object value)
        => variables[key] = value;
    public T GetLongTermVariable<T>(string key, T defaultValue)
    {
        if (variables.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return defaultValue;
    }

    // --------------------------------------------------------------------------

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
    private IEnumerable<T> GetPool<T>(string key)
    {
        // var pool =
        //     pools.Get<Dictionary<int, T>>(key)
        //     ?? throw new PoolDoesNotExistException(key);
        var pool = pools.Get<Dictionary<int, T>>(key);
        if (pool is null)
        {
            pool = [];
            pools.Store(key, () => pool);
        }
        return pool.Values;
    }

    public IEnumerable<T> GetPool<T>()
        => GetPool<T>(typeof(T).FullName!);

    public Dictionary<int, T> GetPoolDictionary<T>()
        => pools.Get<Dictionary<int, T>>(typeof(T).FullName!) ?? [];

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

    public IEnumerable<int> GetPoolIds<T>(Func<T, bool> predicate)
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        if (pool is null)
        {
            pool = [];
            pools.Store(typeof(T).FullName!, () => pool);
        }
        return pool.Where(a => predicate(a.Value)).Select(a => a.Key);
    }

    public (bool, T?) GetFromPool<T>(int id)
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        if (pool is null)
        {
            pool = [];
            pools.Store(typeof(T).FullName!, () => pool);
        }
        if (!pool.TryGetValue(id, out T? value))
        {
            return (false, default);
        }
        return (true, pool[id]);
    }


    public T StoreInPool<T>(int id, T value)
    {
        var key = typeof(T).FullName!;
        var pool = pools.Get<Dictionary<int, T>>(key);
        if (pool is null)
        {
            pool = [];
            pools.Store(key, () => pool);
        }
        pool[id] = value;
        return value;
    }

    public void ClearPool<T>()
    {
        var pool = pools.Get<Dictionary<int, T>>(typeof(T).FullName!);
        pool?.Clear();
    }
    // --------------------------------------------------------------------------
}
