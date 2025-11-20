namespace QuickCheckr.UnderTheHood;

public class ShortTermMemory
{
    // --------------------------------------------------------------------------
    // Execution Tracking
    private readonly List<int> numbers = [];
    public IReadOnlyList<int> Numbers => numbers;
    private int currentExecutionNumber = 0;

    public int Count()
    {
        return numbers.Count;
    }

    public void Remove(int executionNumber)
    {
        numbers.Remove(executionNumber);
        // Should we remove the memory bank and the actions?
    }

    public int StartNewExecution()
    {
        currentExecutionNumber++;
        numbers.Add(currentExecutionNumber);
        inputs[currentExecutionNumber] = new MemoryBank();
        actions[currentExecutionNumber] = [];
        return currentExecutionNumber;
    }

    public void GotoExecution(int executionNumber)
        => currentExecutionNumber = executionNumber;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Inputs
    private readonly Dictionary<int, MemoryBank> inputs = [];
    public T? StoreInput<T>(string key, Func<T> factory)
    {
        var bank = inputs[currentExecutionNumber];
        return bank.Store(key, factory);
    }
    //public T GetInput<T>(string key) => inputs[currentExecutionNumber].Get<T>(key)!;
    public IReadOnlyDictionary<string, object?> GetInputs() => inputs[currentExecutionNumber].Values;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    private readonly Dictionary<int, HashSet<string>> actions = [];
    public void StoreAction(string key)
    {
        var keys = actions[currentExecutionNumber];
        keys.Add(key);
    }
    public IReadOnlyList<string> GetActions() => [.. actions[currentExecutionNumber].Order()];
    // --------------------------------------------------------------------------
}
