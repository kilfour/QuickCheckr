using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Warnings;
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
    {
        var value = ShortTermMemory.StoreInput(KeyGuardian.Inputs.Check(key), factory);
        if (IsConfigRun)
        {
            ShortTermMemory.StoreOriginal(key, value);
        }
        return value;
    }
    // Input Showrs
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
    {
        if (IsConfigRun)
            ShortTermMemory.StoreInputShowr(showr);
    }
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
    public bool IsConfigRun { get; set; } = false;
    public void StoreShrinker(string key, Func<IShrinkOption[]> factory)
        => ShortTermMemory.StoreShrinker(key, factory);
    public IEnumerable<IShrinkOption>? GetShrinker(int executionId, string key, Type type)
        => ShortTermMemory.GetShrinker(executionId, key, type);
    public bool HasShrinker(int executionId, string key, Type type)
    {
        var options = GetShrinker(executionId, key, type);
        return options is not null && options.Any();
    }
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<Reducer<T>> reducer)
        => ShortTermMemory.StoreReducer(key, reducer);
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Warnings
    public Warning? StoreWarning(string key, Func<Warning> factory)
        => ShortTermMemory.StoreWarning(key, factory);
}
