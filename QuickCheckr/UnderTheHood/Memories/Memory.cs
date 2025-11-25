using QuickCheckr.UnderTheHood.Memories.Structure;

namespace QuickCheckr.UnderTheHood.Memories;

public class Memory
{
    public int StartNewExecution()
    {
        return ShortTermMemory.StartNewExecution();
    }

    public void Reset()
    {
        LongTermMemory.Reset();
    }

    public void GotoExecution(int executionNumber)
    {
        ShortTermMemory.GotoExecution(executionNumber);
    }

    public IReadOnlyList<int> Numbers => ShortTermMemory.Numbers;
    public int Count() => ShortTermMemory.Count();
    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries() => ShortTermMemory.Entries();
    public void Remove(int executionNumber) => ShortTermMemory.Remove(executionNumber);
    public IDisposable ScopedSwap(int executionId, string key, object value)
        => ShortTermMemory.ScopedSwap(executionId, key, value);
    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => ShortTermMemory.NestedValue(executionId, swapper);

    // ==========================================================================
    // Long Term
    // == 
    public LongTermMemory LongTermMemory { get; } = new();
    // --------------------------------------------------------------------------
    // Stashed
    public T StoreStashed<T>(string key, Func<T> factory)
        => LongTermMemory.StoreStashed(key, factory);
    public T GetStashed<T>(string key)
        => LongTermMemory.GetStashed<T>(key);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Pool
    public IEnumerable<T> GetPool<T>()
        => LongTermMemory.GetPool<T>();
    public IEnumerable<int> GetPoolIds<T>()
        => LongTermMemory.GetPoolIds<T>();
    public T? GetFromPool<T>(int id)
        => LongTermMemory.GetFromPool<T>(id);
    public T StoreInPool<T>(int id, T value)
        => LongTermMemory.StoreInPool(id, value);
    // --------------------------------------------------------------------------


    // ==========================================================================
    // Short Term
    // == 
    public ShortTermMemory ShortTermMemory { get; } = new();
    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreInput(key, factory);

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => ShortTermMemory.StoreAction(key);
    public bool HasAction(string key)
        => ShortTermMemory.HasAction(key);
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => ShortTermMemory.StoreTrace(key, factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreVariable(key, factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinker<T>(string key, Func<T, IEnumerable<T>> shrinker)
        => ShortTermMemory.StoreShrinker(key, shrinker);
    public Func<T, IEnumerable<T>> GetShrinker<T>(string key)
        => ShortTermMemory.GetShrinker<T>(key);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => ShortTermMemory.StoreReducer(key, reducer);
}
