namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);
    public IReadOnlyDictionary<string, object?> GetInputs() => inputs.Values;
    public IDisposable ScopedSwap(string key, object value)
        => inputs.ScopedSwap(key, value);
    public IDisposable NestedValue(MemoryLens swapper)
        => inputs.NestedValue(swapper);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    public void RemoveAction(string key) => actions.Remove(key);
    public void StoreAction(string key) => actions.Add(key);
    public bool HasAction(string key) => actions.Contains(key);
    public IReadOnlyList<string> GetActions() => [.. actions.Order()];
    private readonly HashSet<string> actions = [];
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Variables
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
    private readonly Dictionary<string, object?> inputShrinkOutcomes = [];
    public void StoreInputShrinkOutcome(string key, object? result)
        => inputShrinkOutcomes[key] = result;
    public IReadOnlyDictionary<string, object?> GetInputShrinkOutcomes()
        => inputShrinkOutcomes; // Order ?
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Shrinkers
    private readonly MemoryBank shrinkers = new();
    public void StoreShrinker<T>(string key, Func<T, IEnumerable<T>> shrinker)
        => shrinkers.Store(key, () => shrinker);
    public Func<T, IEnumerable<T>> GetShrinker<T>(string key)
        => shrinkers.Get<Func<T, IEnumerable<T>>>(key)!;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Reducers
    private readonly MemoryBank reducers = new();
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => reducers.Store(key, () => reducer);
    public Func<T, IEnumerable<T>> GetReducer<T>(string key)
        => reducers.Get<Func<T, IEnumerable<T>>>(key)!;
    // --------------------------------------------------------------------------
}
