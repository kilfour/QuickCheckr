using QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ShortTermMemory
{
    private readonly Dictionary<int, ExecutionMemory> memories = [];
    private ExecutionMemory Current => memories[CurrentExecutionNumber];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys];

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ExecutionMemory();
        ClearGuards();
        return CurrentExecutionNumber;
    }

    public void Remove(int executionNumber)
        => memories.Remove(executionNumber);

    public IDisposable ScopedSwap(int executionId, string key, object value)
        => memories[executionId].ScopedSwap(key, value);
    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => memories[executionId].NestedValue(swapper);

    public void GotoExecution(int executionNumber)
    {
        ClearGuards();
        CurrentExecutionNumber = executionNumber;
    }

    public ExecutionMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));

    // --------------------------------------------------------------------------
    // Guards
    private DuplicateKeyWriteGuardForInputs inputGuard = new();
    private void ClearGuards()
    {
        inputGuard = new();
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => Current.StoreInput(inputGuard.Check(key), factory);

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => Current.StoreAction(key);
    public bool HasAction(string key)
        => Current.GetActions().Contains(key);
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => Current.StoreTrace(key, factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => Current.StoreVariable(key, factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinker<T>(string key, Func<T, IEnumerable<T>> shrinker)
        => Current.StoreShrinker(key, shrinker);
    public Func<T, IEnumerable<T>> GetShrinker<T>(string key)
        => Current.GetShrinker<T>(key);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => Current.StoreReducer(key, reducer);
}
