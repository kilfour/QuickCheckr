using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
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
    private readonly HashSet<string> actions = [];
    public void StoreAction(string key) => actions.Add(key);
    public IReadOnlyList<string> GetActions() => [.. actions.Order()];
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
    private readonly TypedMemoryBank<string> traces = new();
    public void StoreTrace(string key, Func<string> factory)
        => traces.Store(key, factory);
    public IReadOnlyDictionary<string, string> GetTraces()
        => traces.Values;
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Pool Traces
    private readonly TypedMemoryBank<string> poolTraces = new();
    public void StorePoolTrace(string key, Func<string> factory)
        => poolTraces.Store(key, factory);
    public IReadOnlyDictionary<string, string> GetPoolTraces()
        => poolTraces.Values;
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Shrink Outcomes
    private readonly Dictionary<string, InputShrinkingClue> inputShrinkOutcomes = [];
    public void StoreInputShrinkOutcome(string key, InputShrinkingClue inputShrinkingClue)
        => inputShrinkOutcomes[key] = inputShrinkingClue;
    public IReadOnlyDictionary<string, InputShrinkingClue> GetInputShrinkOutcomes()
        => inputShrinkOutcomes;
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Redux Outcomes
    private readonly Dictionary<string, string> reduxOutcomes = [];
    public void StoreInputRedux(string key, string result)
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
    private readonly TypedMemoryBank<IShrinkOption[]> shrinkers = new();
    public void StoreShrinker(string key, Func<IShrinkOption[]> factory)
        => shrinkers.Store(key, factory);
    public IEnumerable<IShrinkOption>? GetShrinker(string key, Type type)
    {
        var options = shrinkers.Get(key);
        if (options == null) return null;
        return options.Where(a => a.TargetType == type);
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Reducers
    private readonly TypedMemoryBank<Func<ReductionResult, Reduction>> reducers = new();
    public void StoreReducer(string key, Func<ReductionResult, Reduction> reducer)
        => reducers.Store(key, () => reducer);
    public Func<ReductionResult, Reduction> GetReducer(string key)
        => reducers.Get(key)!;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Warnings
    private readonly TypedMemoryBank<string> warnings = new();
    public string? StoreWarning(string key, Func<string> factory)
        => warnings.Store(key, factory);
    public IReadOnlyList<string> GetWarnings()
        => [.. warnings.Values.Values];
    // --------------------------------------------------------------------------
}
