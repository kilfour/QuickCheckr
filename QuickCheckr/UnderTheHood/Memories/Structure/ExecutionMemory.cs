using QuickCheckr;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

internal sealed class ExecutionMemory
{
    // --------------------------------------------------------------------------
    // Case Selection
    private readonly List<int> deliberationScores = [];
    private readonly List<Func<CaseFile, int>> runDeliberationScores = [];
    private int? deliberationTarget;
    private readonly List<object?> distinctions = [];
    private readonly List<bool> caseRejections = [];
    internal void StoreDeliberation(int score)
        => deliberationScores.Add(score);
    internal IReadOnlyList<int> GetDeliberationScores() => deliberationScores;
    internal void StoreRunDeliberation(Func<CaseFile, int> score)
        => runDeliberationScores.Add(score);
    internal IReadOnlyList<Func<CaseFile, int>> GetRunDeliberationScores()
        => runDeliberationScores;
    internal void StoreDeliberationTarget(int target)
    {
        if (deliberationTarget.HasValue)
            throw new InvalidOperationException(
                "Trackr.DeliberateUntil may only be used once per run.");
        deliberationTarget = target;
    }
    internal int? GetDeliberationTarget() => deliberationTarget;
    internal void StoreDistinction(object? distinction)
        => distinctions.Add(distinction);
    internal IReadOnlyList<object?> GetDistinctions() => distinctions;
    internal void StoreCaseRejection(bool reject)
        => caseRejections.Add(reject);
    internal IReadOnlyList<bool> GetCaseRejections() => caseRejections;
    internal void ClearCaseSelection()
    {
        deliberationScores.Clear();
        runDeliberationScores.Clear();
        deliberationTarget = null;
        distinctions.Clear();
        caseRejections.Clear();
    }
    // --------------------------------------------------------------------------
    // Inputs
    private readonly OverlayMemoryBank inputs = new();
    public T? StoreInput<T>(string key, Func<T> factory)
        => inputs.Store(key, factory);

    public TypedValue GetInput(string key)
       => inputs.Values[key];
    public object? GetCurrentInputValue(string key)
        => inputs.GetCurrentValue(key);
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
        try
        {
            if (lenses.TryGetValue(key, out List<Testimony>? testimonies))
            {
                foreach (var item in testimonies)
                {
                    disposables.Add(inputs.ScopedSwap(item.Lenses, key, item.Witness!));
                }
            }
            disposables.Add(inputs.ScopedSwap(key, value));
        }
        catch
        {
            DisposeInReverseOrder(disposables);
            throw;
        }

        return new DisposableAction(() => DisposeInReverseOrder(disposables));
    }

    private static void DisposeInReverseOrder(List<IDisposable> disposables)
    {
        for (var index = disposables.Count - 1; index >= 0; index--)
            disposables[index].Dispose();
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
    private readonly HashSet<string> shrinkableActions = [];
    public void StoreShrinkableAction(string key) => shrinkableActions.Add(key);
    public IReadOnlyList<string> GetShrinkableActions() => [.. shrinkableActions.Order()];
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
    private readonly TypedMemoryBank<string[]> traces = new();
    public void StoreTrace(string key, Func<string[]> factory)
        => traces.Store(key, factory);
    public IReadOnlyDictionary<string, string[]> GetTraces()
        => traces.Values;

    private readonly TypedMemoryBank<string[]> finalTraces = new();
    public void StoreFinalTrace(string key, Func<string[]> factory)
        => finalTraces.Store(key, factory);
    public IReadOnlyDictionary<string, string[]> GetFinalTraces()
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

internal sealed record CaseSelectionResult(
    int? ExecutionDeliberationScore,
    IReadOnlyList<Func<CaseFile, int>> RunDeliberationScores,
    int? DeliberationTarget,
    DistinctionKey? Distinction,
    bool RejectCase);

internal sealed class DistinctionKey(IReadOnlyList<object?> values) : IEquatable<DistinctionKey>
{
    internal IReadOnlyList<object?> Values { get; } = values;

    public bool Equals(DistinctionKey? other)
        => other is not null && Values.SequenceEqual(other.Values);

    public override bool Equals(object? obj)
        => Equals(obj as DistinctionKey);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in Values)
            hash.Add(value);
        return hash.ToHashCode();
    }
}
