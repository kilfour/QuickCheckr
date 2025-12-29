using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);
    public Type GetActualType(string key) => inputs.GetActualType(key);
    public IReadOnlyDictionary<string, object?> GetInputValues() => inputs.Values.Select(a => (a.Key, a.Value.Value)).ToDictionary(a => a.Key, a => a.Value);
    public IReadOnlyDictionary<string, TypedValue> GetInputs() => inputs.Values;
    public IDisposable ScopedSwap(string key, object value)
        => inputs.ScopedSwap(key, value);
    public IDisposable NestedValue(MemoryLens swapper)
        => inputs.NestedValue(swapper);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key) => actions.Add(key);
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
    // Traces
    private readonly MemoryBank poolTraces = new();
    public void StorePoolTrace(string key, Func<string> factory)
        => poolTraces.Store(key, factory);
    public IReadOnlyDictionary<string, object?> GetPoolTraces()
        => poolTraces.Values;
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
    public Func<object, IEnumerable<object>> GetShrinker(string key)
        => shrinkers.Get<Func<object, IEnumerable<object>>>(key)!;
    // --------------------------------------------------------------------------
    private Dictionary<string, ShrinkOption[]> shrinkerrs = [];
    public void StoreShrinkerr<T>(string key, ShrinkOption[] options)
        => shrinkerrs[key] = options;
    public ShrinkOption[]? GetShrinkerr(string key)
        => shrinkerrs.TryGetValue(key, out ShrinkOption[]? value) ? value : null;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Reducers
    private readonly MemoryBank reducers = new();
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => reducers.Store(key, () => reducer);
    public Func<T, IEnumerable<T>> GetReducer<T>(string key)
        => reducers.Get<Func<T, IEnumerable<T>>>(key)!;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Warnings
    private readonly MemoryBank warnings = new();
    public T? StoreWarning<T>(string key, Func<T> factory)
        => warnings.Store(key, factory);
    public IReadOnlyList<string> GetWarnings()
        => [.. warnings.Values.Values.Cast<string>()];
    // --------------------------------------------------------------------------
}
