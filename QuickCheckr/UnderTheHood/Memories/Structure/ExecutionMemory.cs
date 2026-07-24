using QuickCheckr;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);

    private readonly MemoryBank fuzzrs = new();
    public T? StoreInputFuzzr<T>(string key, Func<FuzzrOf<T>> fuzzrFactory, Func<T> factory)
    {
        fuzzrs.Store(key, () => fuzzrFactory().AsObject());
        return inputs.Store(key, factory);
    }
    public FuzzrOf<object>? GetFuzzr(string key)
        => fuzzrs.Get<FuzzrOf<object>>(key);

    public TypedValue GetInput(string key)
       => inputs.Values[key];
    public IReadOnlyDictionary<string, TypedValue> GetInputValues()
        => inputs.Values.Select(a => (a.Key, a.Value)).ToDictionary(a => a.Key, a => a.Value);
    public IReadOnlyDictionary<string, TypedValue> GetInputs() => inputs.Values;
    public IDisposable ScopedSwap(string key, object value)
        => inputs.ScopedSwap(key, value);
    public IDisposable NestedValue(MemoryLens swapper)
        => inputs.NestedValue(swapper);
    public void Commit(string key, object value)
        => inputs.Commit(key, value);
    public record Testimony(Stack<MemoryLens> Lenses, object? Witness);
    public Dictionary<string, List<Testimony>> lenses = [];
    public void Testify(string key, object value)
    {
        if (!lenses.ContainsKey(key)) lenses[key] = [];
        lenses[key].Add(new Testimony(inputs.GetCurrentSwappers(), value));
    }

    public IDisposable WithTestimony(string key, object value)
    {
        var disposables = new List<IDisposable>();
        // foreach (var item in lenses)
        // {
        //     foreach (var lens in item.Value)
        //         disposables.Add(inputs.ScopedSwap(lens.Lenses, key, lens.Witness!));
        // }
        if (lenses.TryGetValue(key, out List<Testimony>? testimonies))
        {
            foreach (var item in testimonies)
            {
                disposables.Add(inputs.ScopedSwap(item.Lenses, key, item.Witness!));
            }
        }
        disposables.Add(inputs.ScopedSwap(key, value));
        return new DisposableAction(() =>
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        });
    }
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
    public void RemoveAction(string key) => actions.Remove(key);
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

    private readonly TypedMemoryBank<string> finalTraces = new();
    public void StoreFinalTrace(string key, Func<string> factory)
        => finalTraces.Store(key, factory);
    public IReadOnlyDictionary<string, string> GetFinalTraces()
        => finalTraces.Values;
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
    private readonly Dictionary<string, InputClassification> inputShrinkClassifications = [];
    public void StoreInputShrinkingClassification(string key, InputClassification inputShrinkingClassification)
        => inputShrinkClassifications[key] = inputShrinkingClassification;
    public IReadOnlyDictionary<string, InputClassification> GetInputShrinkingClassifications()
        => inputShrinkClassifications;
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Shrinkers
    private readonly TypedMemoryBank<Shrinker[]> conditionalShrinkers = new();
    public void StoreShrinkers(string key, Shrinker[] shrinkers)
        => conditionalShrinkers.Store(key, () => shrinkers);
    public Shrinker[]? GetShrinkers(string key, Type type)
    {
        StoreClassificationUsed(key, type);
        return conditionalShrinkers.Get(key);
    }

    private readonly Dictionary<string, List<Type>> shrinkersUsed = [];
    private void StoreClassificationUsed(string key, Type type)
    {
        if (shrinkersUsed.TryGetValue(key, out List<Type>? value))
            value.Add(type);
        else
            shrinkersUsed[key] = [type];
    }

    public (bool, List<Type>) HasUnusedClassifiers(string key)
    {
        var shrinkers = conditionalShrinkers.Get(key);
        if (shrinkers is null) return (false, []);

        var options = shrinkers.SelectMany(a => a.Classifiers).ToList();
        if (options == null) return (false, []);
        if (shrinkersUsed.TryGetValue(key, out List<Type>? used))
        {
            options = [.. options.Where(a => !used.Contains(a.TargetType))];
        }
        if (options.Count != 0)
            return (true, [.. options.Select(a => a.TargetType)]);
        return (false, []);
    }
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
