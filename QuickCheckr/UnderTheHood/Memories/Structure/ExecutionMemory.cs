using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);
    public TypedValue GetInput(string key)
       => inputs.Values[key];
    public IReadOnlyDictionary<string, TypedValue> GetInputValues()
        => inputs.Values.Select(a => (a.Key, a.Value)).ToDictionary(a => a.Key, a => a.Value);
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
    // Shrink Classifications
    private readonly Dictionary<string, InputShrinkingClassification> inputShrinkClassifications = [];
    public void StoreInputShrinkingClassification(string key, InputShrinkingClassification inputShrinkingClassification)
        => inputShrinkClassifications[key] = inputShrinkingClassification;
    public IReadOnlyDictionary<string, InputShrinkingClassification> GetInputShrinkingClassifications()
        => inputShrinkClassifications;
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Redux Outcomes
    private readonly Dictionary<string, object> reduxOutcomes = [];
    public void StoreInputRedux(string key, object result)
        => reduxOutcomes[key] = result;
    public Maybe<object> GetInputReduxForKey(string key)
    {
        if (reduxOutcomes.TryGetValue(key, out object? value))
        {
            return Maybe<object>.Just(value);
        }
        return Maybe<object>.Nothing();
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Shrinkers
    private readonly TypedMemoryBank<IShrinkOption[]> shrinkers = new();
    public void StoreShrinker(string key, Func<IShrinkOption[]> factory)
        => shrinkers.Store(key, factory);
    public IEnumerable<IShrinkOption> GetShrinkOptions(string key, Type type)
    {
        var options = shrinkers.Get(key);
        if (options == null) return [];
        StoreShrinkerUsed(key, type);
        return options.Where(a => a.TargetType == type);
    }
    private readonly Dictionary<string, List<Type>> shrinkersUsed = [];
    private void StoreShrinkerUsed(string key, Type type)
    {
        if (shrinkersUsed.TryGetValue(key, out List<Type>? value))
            value.Add(type);
        else
            shrinkersUsed[key] = [type];
    }
    public (bool, List<Type>) HasUnusedShrinkers(string key)
    {
        var options = shrinkers.Get(key);
        if (options == null) return (false, []);
        if (shrinkersUsed.TryGetValue(key, out List<Type>? used))
        {
            options = [.. options.Where(a => !used.Contains(a.TargetType))];
        }
        if (options.Length != 0)
            return (true, [.. options.Select(a => a.TargetType)]);
        return (false, []);
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Reducers
    private readonly TypedMemoryBank<Func<Func<ReductionResult, Reduction>>> reducers = new();
    public void StoreReducer<T>(string key, Func<Reducer<T>> reducer)
        => reducers.Store(key, () => () => // looks ugly, but is needed
            {
                Reducer<T> f = reducer();
                return a => f(a);
            });
    public Func<Func<ReductionResult, Reduction>> GetReducer(string key)
        => reducers.Get(key)!;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Warnings
    private readonly TypedMemoryBank<Warning> warnings = new();
    public Warning? StoreWarning(string key, Func<Warning> factory)
        => warnings.Store(key, factory);
    public IReadOnlyList<Warning> GetWarnings()
        => [.. warnings.Values.Values];
    // --------------------------------------------------------------------------
}
