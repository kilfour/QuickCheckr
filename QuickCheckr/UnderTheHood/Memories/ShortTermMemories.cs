
namespace QuickCheckr.UnderTheHood.Memories;

public class ShortTermMemories
{
    private readonly Dictionary<int, ShortTermMemory> memories = [];
    private ShortTermMemory Current => memories[CurrentExecutionNumber];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys];

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ShortTermMemory();
        inputAccessPerExecution = [];
        return CurrentExecutionNumber;
    }

    public void Remove(int executionNumber)
        => memories.Remove(executionNumber);

    public void GotoExecution(int executionNumber)
    {
        inputAccessPerExecution = [];
        CurrentExecutionNumber = executionNumber;
    }

    public ShortTermMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ShortTermMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));

    // --------------------------------------------------------------------------
    // Inputs
    private List<string> inputAccessPerExecution = [];
    public T? StoreInput<T>(string key, Func<T> factory)
    {
        if (inputAccessPerExecution.Contains(key))
        {
            throw new InvalidOperationException($"Input {key} has already been used in this execution.");
        }
        inputAccessPerExecution.Add(key);
        return Current.StoreInput(key, factory);
    }

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
