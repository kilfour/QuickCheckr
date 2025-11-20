namespace QuickCheckr.UnderTheHood;

public class ShortTermMemory
{
    // --------------------------------------------------------------------------
    // Execution Tracking
    private readonly List<int> numbers = [];
    public IReadOnlyList<int> Numbers => numbers;
    private int currentExecutionNumber = 0;
    public int Count() => numbers.Count;

    public int StartNewExecution()
    {
        currentExecutionNumber++;
        numbers.Add(currentExecutionNumber);
        inputs[currentExecutionNumber] = new MemoryBank();
        actions[currentExecutionNumber] = [];
        return currentExecutionNumber;
    }

    public void Remove(int executionNumber)
    {
        numbers.Remove(executionNumber);
    }

    public void GotoExecution(int executionNumber)
        => currentExecutionNumber = executionNumber;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => CurrentInputs().Store(key, factory);

    public IReadOnlyDictionary<string, object?> GetInputs()
        => CurrentInputs().Values;

    private readonly Dictionary<int, MemoryBank> inputs = [];

    private MemoryBank CurrentInputs() =>
        inputs[currentExecutionNumber];
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    public void RemoveAction(string key)
        => CurrentActions().Remove(key);

    public void StoreAction(string key)
        => CurrentActions().Add(key);

    public bool HasAction(string key)
        => CurrentActions().Contains(key);

    public IReadOnlyList<string> GetActions()
        => [.. CurrentActions().Order()];

    private HashSet<string> CurrentActions()
        => actions[currentExecutionNumber];

    private readonly Dictionary<int, HashSet<string>> actions = [];
    // --------------------------------------------------------------------------
}
