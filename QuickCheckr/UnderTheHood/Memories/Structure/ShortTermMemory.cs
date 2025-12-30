using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ShortTermMemory
{
    private readonly Dictionary<int, ExecutionMemory> memories = [];
    private ExecutionMemory Current => memories[CurrentExecutionNumber];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys.Where(a => a != skipExecutionNumber)];

    public int Reset()
        => CurrentExecutionNumber = Numbers[0];

    private int skipExecutionNumber = -1;

    public IDisposable ScopedExecutionRemoval(int executionNumber)
    {
        skipExecutionNumber = executionNumber;
        return new DisposableAction(() => skipExecutionNumber = -1);
    }

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ExecutionMemory();
        return CurrentExecutionNumber;
    }

    public void GotoExecution(int executionNumber)
        => CurrentExecutionNumber = executionNumber;

    public int NextExecution()
        => CurrentExecutionNumber = Numbers.FirstOrDefault(a => a > CurrentExecutionNumber);

    public void Remove(int executionNumber)
        => memories.Remove(executionNumber);

    public IDisposable ScopedSwap(int executionId, string key, object value)
        => memories[executionId].ScopedSwap(key, value);
    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => memories[executionId].NestedValue(swapper);


    public ExecutionMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));

    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => Current.StoreInput(key, factory);
    // --------------------------------------------------------------------------
    // Input Showrs
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
        => Current.StoreInputShowr(showr);
    public void StoreInputShowrForKey(string key)
        => Current.StoreInputShowrForKey(key);
    public Func<ManOfWealthAndTaste>? GetInputShowrForKey(int executionId, string key)
        => memories[executionId].GetInputShowrForKey(key);

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => Current.StoreAction(key);
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => Current.StoreTrace(key, factory);
    // --------------------------------------------------------------------------
    // Pool Traces
    public void StorePoolTrace(string key, Func<string> factory)
        => Current.StorePoolTrace(key, factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => Current.StoreVariable(key, factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinkerForType<T>(IShrinkOption[] options)
        => Current.StoreShrinkerForType<T>(options);
    public void StoreShrinkerForKey<T>(string key)
        => Current.StoreShrinkerForKey<T>(key);
    public IShrinkOption[]? GetShrinkerForKey(int executionId, string key, Type type)
        => memories[executionId].GetShrinkerForKey(key, type);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => Current.StoreReducer(key, reducer);
    // --------------------------------------------------------------------------
    // Warnings
    public T? StoreWarning<T>(string key, Func<T> factory)
        => Current.StoreWarning(key, factory);
    // --------------------------------------------------------------------------
}
