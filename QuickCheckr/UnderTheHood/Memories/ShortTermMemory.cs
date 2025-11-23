using QuickCheckr.UnderTheHood.Memories;

namespace QuickCheckr.UnderTheHood;


public class ShortTermMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();

    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);

    public IReadOnlyDictionary<string, object?> GetInputs()
        => inputs.Values;

    public IDisposable ScopedSwap(string key, object value)
        => inputs.ScopedSwap(key, value);

    public IDisposable NestedValue(MemoryLens swapper)
        => inputs.NestedValue(swapper);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    public void RemoveAction(string key)
        => actions.Remove(key);

    public void StoreAction(string key)
        => actions.Add(key);

    public bool HasAction(string key)
        => actions.Contains(key);

    public IReadOnlyList<string> GetActions()
        => [.. actions.Order()];
    private readonly HashSet<string> actions = [];
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // OneOf Operations & maybe future variables
    private readonly MemoryBank variables = new();

    public T? StoreVariable<T>(string key, Func<T> factory)
        => variables.Store(key, factory);

    public IReadOnlyDictionary<string, object?> GetVariables()
        => variables.Values;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Traces
    private readonly MemoryBank traces = new();

    public void StoreTrace(string key, Func<string> factory)
        => traces.Store(key, factory);

    public IReadOnlyDictionary<string, object?> GetTraces()
        => traces.Values;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Shrink Outcomes
    public void StoreInputShrinkOutcome(string key, object? result)
        => inputShrinkOutcomes[key] = result;

    public IReadOnlyDictionary<string, object?> GetInputShrinkOutcomes()
        => inputShrinkOutcomes; // Order ?

    private readonly Dictionary<string, object?> inputShrinkOutcomes = [];
    // --------------------------------------------------------------------------
}
