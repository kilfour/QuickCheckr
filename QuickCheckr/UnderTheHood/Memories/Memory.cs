using QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Memories;

public class Memory
{
    public int StartNewExecution()
    {
        ClearGuards();
        return ShortTermMemory.StartNewExecution();
    }

    public void GotoExecution(int executionNumber)
    {
        ClearGuards();
        ShortTermMemory.GotoExecution(executionNumber);
    }

    public int NextExecution()
    {
        ClearGuards();
        return ShortTermMemory.NextExecution();
    }

    public int Reset()
    {
        ClearGuards();
        LongTermMemory.Reset();
        return ShortTermMemory.Reset();
    }

    public IReadOnlyList<int> Numbers => ShortTermMemory.Numbers;
    public int Count() => ShortTermMemory.Count();
    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries() => ShortTermMemory.Entries();
    public void Remove(int executionNumber) => ShortTermMemory.Remove(executionNumber);
    public IDisposable ScopedSwap(int executionId, string key, object value)
        => ShortTermMemory.ScopedSwap(executionId, key, value);
    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => ShortTermMemory.NestedValue(executionId, swapper);
    // --------------------------------------------------------------------------
    // Guards
    private DuplicateKeyWriteGuardForInputs inputGuard = new();
    private DuplicateKeyWriteGuardForActs actGuard = new();
    public DuplicateKeyWriteGuardForSpecs SpecGuard { get; private set; } = new();
    private DuplicateKeyWriteGuardForTraces traceGuard = new();
    private DuplicateKeyWriteGuardForVariables variableGuard = new();
    private DuplicateKeyWriteGuardForStashed stashedGuard = new();
    //private DuplicateKeyWriteGuardForPools poolGuard = new();
    private void ClearGuards()
    {
        inputGuard = new();
        actGuard = new();
        SpecGuard = new();
        traceGuard = new();
        variableGuard = new();
        stashedGuard = new();
        //poolGuard = new();
    }
    // --------------------------------------------------------------------------


    // ==========================================================================
    // Long Term
    // == 
    public LongTermMemory LongTermMemory { get; } = new();
    // --------------------------------------------------------------------------
    // Stashed
    public T StoreStashed<T>(string key, Func<T> factory)
        => LongTermMemory.StoreStashed(stashedGuard.Check(key), factory);
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
        => ShortTermMemory.StoreInput(inputGuard.Check(key), factory);

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => ShortTermMemory.StoreAction(actGuard.Check(key));
    public bool HasAction(string key)
        => ShortTermMemory.HasAction(key);
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => ShortTermMemory.StoreTrace(traceGuard.Check(key), factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreVariable(variableGuard.Check(key), factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinker<T>(string key, Func<T, IEnumerable<T>> shrinker)
        => ShortTermMemory.StoreShrinker(key, shrinker);
    public Func<T, IEnumerable<T>> GetShrinker<T>(int executionId, string key)
        => ShortTermMemory.GetShrinker<T>(executionId, key);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => ShortTermMemory.StoreReducer(key, reducer);
}
