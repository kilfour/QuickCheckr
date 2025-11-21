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
        inputShrinkOutcomes[currentExecutionNumber] = [];
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
    // --------------
    // Swap Stuff
    // --------------
    public class MemoryLens(
        Func<object, object> get,
        Func<object, object, object> set)
    {
        public Func<object, object> Get { get; init; } = get;
        public Func<object, object, object> Set { get; init; } = set;
    }

    private readonly Stack<MemoryLens> swappers = new();

    public IDisposable NestedValue(MemoryLens swapper)
    {
        swappers.Push(swapper);
        return new DisposableAction(() => swappers.Pop());
    }

    public IDisposable ScopedSwap(int executionId, string key, object value)
    {
        var memoryBank = inputs[executionId];
        Func<object> getValue = () => memoryBank.Get<object>(key);
        var setSteps = new Stack<(MemoryLens lens, object container)>();
        foreach (var lens in swappers.Reverse())
        {
            var currentGet = getValue;
            getValue = () => lens.Get(currentGet());
            var container = currentGet();
            setSteps.Push((lens, container));
        }
        var oldValue = getValue();
        object setValue(object newLeaf)
        {
            var updated = newLeaf;
            foreach (var (lens, container) in setSteps)
                updated = lens.Set(container, updated);
            return updated;
        }
        memoryBank.Set(key, setValue(value));
        return new DisposableAction(() => memoryBank.Set(key, setValue(oldValue)));
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    // public void RemoveAction(string key)
    //     => CurrentInputShrinkOutcomes().Remove(key);

    public void StoreInputShrinkOutcome(string key, object? result)
        => CurrentInputShrinkOutcomes()[key] = result;

    // public bool HasAction(string key)
    //     => CurrentInputShrinkOutcomes().Contains(key);

    public IReadOnlyDictionary<string, object?> GetInputShrinkOutcomes()
        => CurrentInputShrinkOutcomes(); // Order ?

    private Dictionary<string, object?> CurrentInputShrinkOutcomes()
        => inputShrinkOutcomes[currentExecutionNumber];

    private readonly Dictionary<int, Dictionary<string, object?>> inputShrinkOutcomes = [];
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
