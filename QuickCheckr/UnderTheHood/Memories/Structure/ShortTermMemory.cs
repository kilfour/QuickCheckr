using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;
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
    public void Commit(int executionId, string key, object value)
        => memories[executionId].Commit(key, value);


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
    public void StoreOriginal(string key, object? value)
        => Current.StoreOriginal(key, value);
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
    // Classifiers
    public void StoreClassification(string key, Func<IClassificationOption[]> factory)
        => Current.StoreClassification(key, factory);
    public IEnumerable<IClassificationOption> GetClassificationOptions(int executionId, string key, Type type)
        => memories[executionId].GetClassificationOptions(key, type);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducerByType<T>(string key, Func<Reducer<T>> reducer)
        => Current.StoreReducerByType(key, reducer);
    public void StoreReducerByType(string key, Type type, Func<Func<ReductionResult, Reduction>> reducer)
        => Current.StoreReducerByType(key, type, reducer);
    public Func<Func<ReductionResult, Reduction>>? GetReducerByType(string key, Type type)
        => Current.GetReducerByType(key, type);
    public void StoreReducerByPath<T>(string key, (string Path, Func<Reducer<T>> Reducer)[] reducers)
        => Current.StoreReducerByPath(key, reducers);
    public Func<Func<ReductionResult, Reduction>>? GetReducerByPath(string key, string path, Type type)
        => Current.GetReducerByPath(key, path, type);
    // --------------------------------------------------------------------------
    // Warnings
    public Warning? StoreWarning(string key, Func<Warning> factory)
        => Current.StoreWarning(key, factory);
    // --------------------------------------------------------------------------
}
