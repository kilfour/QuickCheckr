using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories;

public class Memory
{
    public KeyGuardian KeyGuardian = new();
    public int StartNewExecution()
    {
        KeyGuardian = new();
        return ShortTermMemory.StartNewExecution();
    }

    public void GotoExecution(int executionNumber)
    {
        KeyGuardian = new();
        ShortTermMemory.GotoExecution(executionNumber);
    }

    public int NextExecution()
    {
        KeyGuardian = new();
        return ShortTermMemory.NextExecution();
    }

    public int Reset()
    {
        KeyGuardian = new();
        LongTermMemory.Reset();
        return ShortTermMemory.Reset();
    }

    public IReadOnlyList<int> Numbers
        => ShortTermMemory.Numbers;
    public int Count()
        => ShortTermMemory.Count();
    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => ShortTermMemory.Entries();
    public void Remove(int executionNumber)
        => ShortTermMemory.Remove(executionNumber);
    public IDisposable ScopedExecutionRemoval(int executionNumber)
        => ShortTermMemory.ScopedExecutionRemoval(executionNumber);
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
        => LongTermMemory.StoreStashed(KeyGuardian.Stash.Check(key), factory);
    public T GetStashed<T>(string key)
        => LongTermMemory.GetStashed<T>(key);
    // --------------------------------------------------------------------------
    // Pool
    public IEnumerable<T> CreatePool<T>()
        => LongTermMemory.CreatePool<T>(KeyGuardian.Pools.Check(typeof(T).FullName!));
    public IEnumerable<T> GetPool<T>()
        => LongTermMemory.GetPool<T>();
    public Dictionary<int, T> GetPoolDictionary<T>()
        => LongTermMemory.GetPoolDictionary<T>();
    public IEnumerable<int> GetPoolIds<T>()
        => LongTermMemory.GetPoolIds<T>();
    public IEnumerable<int> GetPoolIds<T>(Func<T, bool> predicate)
        => LongTermMemory.GetPoolIds(predicate);
    public (bool, T?) GetFromPool<T>(int id)
        => LongTermMemory.GetFromPool<T>(id);
    public T StoreInPool<T>(int id, string key, T value)
        => LongTermMemory.StoreInPool(id, key, value);
    public void ClearPool<T>()
        => LongTermMemory.ClearPool<T>();
    // --------------------------------------------------------------------------


    // ==========================================================================
    // Short Term
    // == 
    public ShortTermMemory ShortTermMemory { get; } = new();
    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreInput(KeyGuardian.Inputs.Check(key), factory);
    // --------------------------------------------------------------------------
    // Input Showrs
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
    {
        if (StoringShrinkers)
            ShortTermMemory.StoreInputShowr(showr);
    }
    public void StoreInputShowrForKey(string key)
    {
        if (StoringShrinkers)
            ShortTermMemory.StoreInputShowrForKey(key);
    }
    public Func<ManOfWealthAndTaste>? GetInputShowrForKey(int executionId, string key)
        => ShortTermMemory.GetInputShowrForKey(executionId, key);
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => ShortTermMemory.StoreAction(KeyGuardian.Acts.Check(key));

    // --------------------------------------------------------------------------
    // Traces
    public bool Tracing { get; set; } = false;
    public void StoreTrace(string key, Func<string> factory)
    {
        if (Tracing)
            ShortTermMemory.StoreTrace(KeyGuardian.Traces.Check(key), factory);
    }
    // --------------------------------------------------------------------------
    // Traces
    public void StorePoolTrace(string key, Func<string> factory)
    {
        if (Tracing)
            ShortTermMemory.StorePoolTrace(KeyGuardian.PoolTraces.Check(key), factory);
    }
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreVariable(KeyGuardian.Variables.Check(key), factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public bool StoringShrinkers { get; set; } = false;
    public void StoreShrinkerForType<T>(IShrinkOption[] options)
    {
        if (StoringShrinkers)
            ShortTermMemory.StoreShrinkerForType<T>(options);
    }
    public void StoreShrinkerForKey<T>(string key)
    {
        if (StoringShrinkers)
            ShortTermMemory.StoreShrinkerForKey<T>(key);
    }
    public IShrinkOption[]? GetShrinkerForKey(int executionId, string key, Type type)
        => ShortTermMemory.GetShrinkerForKey(executionId, key, type);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => ShortTermMemory.StoreReducer(key, reducer);
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Warnings
    public T? StoreWarning<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreWarning(key, factory);
}
