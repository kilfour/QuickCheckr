using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);
    public IReadOnlyDictionary<string, object?> GetInputValues()
        => inputs.Values.Select(a => (a.Key, a.Value.Value)).ToDictionary(a => a.Key, a => a.Value);
    public IReadOnlyDictionary<string, TypedValue> GetInputs() => inputs.Values;
    public IDisposable ScopedSwap(string key, object value)
        => inputs.ScopedSwap(key, value);
    public IDisposable NestedValue(MemoryLens swapper)
        => inputs.NestedValue(swapper);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Input Showrs
    private Func<ManOfWealthAndTaste>? currentInputShowr = null;
    private readonly Dictionary<string, string> originals = [];
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
        => currentInputShowr = showr;
    public void StoreOriginal(string key, object? value)
    {
        if (currentInputShowr == null) return;
        originals[key] = currentInputShowr().IntroduceThis(value);
    }
    public Maybe GetOriginalForKey(string key)
    {
        if (originals.TryGetValue(key, out string? value))
        {
            return Maybe.Just(value);
        }
        return Maybe.Nothing();
    }
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
    // Pool Traces
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
        => inputShrinkOutcomes;
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Redux Outcomes
    private readonly Dictionary<string, string?> reduxOutcomes = [];
    public void StoreInputRedux(string key, string? result)
        => reduxOutcomes[key] = result;
    public Maybe GetInputReduxForKey(string key)
    {
        if (reduxOutcomes.TryGetValue(key, out string? value))
        {
            return Maybe.Just(value);
        }
        return Maybe.Nothing();
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Shrinkers
    private readonly Dictionary<Type, IShrinkOption[]> shrinkersPerType = [];
    private readonly Dictionary<string, Dictionary<Type, IShrinkOption[]>> shrinkersPerKey = [];
    public void StoreShrinkerForType<T>(IShrinkOption[] options)
        => shrinkersPerType[typeof(T)] = options;
    public void StoreShrinkerForKey<T>(string key)
    {
        shrinkersPerKey[key] = shrinkersPerType.ToDictionary();
    }
    public IShrinkOption[]? GetShrinkerForKey(string key, Type type)
    {
        if (shrinkersPerKey.TryGetValue(key, out Dictionary<Type, IShrinkOption[]>? dictionary))
        {
            if (dictionary.TryGetValue(type, out IShrinkOption[]? options))
                return options;
        }
        return null;
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Reducers
    private readonly MemoryBank reducers = new();
    public void StoreReducer(string key, Func<ReductionResult, Reduction> reducer)
        => reducers.Store(key, () => reducer);
    public Func<ReductionResult, Reduction> GetReducer(string key)
        => reducers.Get<Func<ReductionResult, Reduction>>(key)!;
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
