using QuickCheckr;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class ShortTermMemory
{
    private readonly Dictionary<int, ExecutionMemory> memories = [];
    private ExecutionMemory Current => memories[CurrentExecutionNumber];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys.Where(a => a != skipExecutionNumber)];

    public int Reset()
        => CurrentExecutionNumber = Numbers[0];

    private int skipExecutionNumber = -1;

    public IDisposable ScopedExecutionRemoval(int executionNumber)
    {
        skipExecutionNumber = executionNumber;
        return new DisposableAction(() => skipExecutionNumber = -1);
    }

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ExecutionMemory();
        return CurrentExecutionNumber;
    }

    public void GotoExecution(int executionNumber)
        => CurrentExecutionNumber = executionNumber;

    public int NextExecution()
        => CurrentExecutionNumber = Numbers.FirstOrDefault(a => a > CurrentExecutionNumber);

    public void Remove(int executionNumber)
        => memories.Remove(executionNumber);

    public IDisposable ScopedSwap(int executionId, string key, object value)
        => memories[executionId].ScopedSwap(key, value);
    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => memories[executionId].NestedValue(swapper);
    public void Commit(int executionId, string key, object value)
        => memories[executionId].Commit(key, value);
    public void Testify(int executionId, string key, object value)
        => memories[executionId].Testify(key, value);
    public IDisposable WithTestimony(int executionId, string key, object value)
        => memories[executionId].WithTestimony(key, value);


    public ExecutionMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));

    // --------------------------------------------------------------------------
    // Case Selection
    internal void StoreDeliberation(int score)
        => Current.StoreDeliberation(score);
    internal void StoreRunDeliberation(Func<CaseFile, int> score)
        => Current.StoreRunDeliberation(score);
    internal void StoreDeliberationTarget(int target)
        => Current.StoreDeliberationTarget(target);
    internal void StoreDistinction(object? distinction)
        => Current.StoreDistinction(distinction);
    internal void ClearCaseSelection()
    {
        foreach (var memory in memories.Values)
            memory.ClearCaseSelection();
    }
    internal CaseSelectionResult? GetCaseSelection()
    {
        var scores = memories.Values
            .SelectMany(memory => memory.GetDeliberationScores())
            .ToList();
        var distinctions = memories.Values
            .SelectMany(memory => memory.GetDistinctions())
            .ToList();
        var runScores = memories.Values
            .SelectMany(memory => memory.GetRunDeliberationScores())
            .ToList();
        if (scores.Count == 0 && runScores.Count == 0 && distinctions.Count == 0)
            return null;

        var targets = memories.Values
            .Select(memory => memory.GetDeliberationTarget())
            .Where(target => target.HasValue)
            .Distinct()
            .ToList();
        if (targets.Count > 1)
            throw new InvalidOperationException(
                "Trackr.DeliberateUntil must use the same target throughout the run.");

        return new CaseSelectionResult(
            scores.Count == 0 ? null : scores.Sum(),
            runScores,
            targets.SingleOrDefault(),
            distinctions.Count == 0 ? null : new DistinctionKey(distinctions));
    }
    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => Current.StoreInput(key, factory);
    public T? StoreInputFuzzr<T>(string key, Func<FuzzrOf<T>> fuzzrFactory, Func<T> factory)
        => Current.StoreInputFuzzr(key, fuzzrFactory, factory);
    public FuzzrOf<object>? GetFuzzr(int executionId, string key)
        => memories[executionId].GetFuzzr(key);
    public object? GetCurrentInputValue(int executionId, string key)
        => memories[executionId].GetCurrentInputValue(key);
    // --------------------------------------------------------------------------
    // Input Showrs
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
        => Current.StoreInputShowr(showr);
    public void StoreOriginal(string key, object? value)
        => Current.StoreOriginal(key, value);
    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => Current.StoreAction(key);
    public void StoreShrinkableAction(string key)
        => Current.StoreShrinkableAction(key);
    public IReadOnlyList<string> GetActions() => Current.GetActions();
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => Current.StoreTrace(key, factory);
    public void StoreFinalTrace(string key, Func<string> factory)
        => Current.StoreFinalTrace(key, factory);
    // --------------------------------------------------------------------------
    // Pool Traces
    public void StorePoolTrace(string key, Func<string> factory)
        => Current.StorePoolTrace(key, factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => Current.StoreVariable(key, factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinkers(string key, Shrinker[] shrinkers)
        => Current.StoreShrinkers(key, shrinkers);
    public Shrinker[]? GetShrinkers(int executionId, string key, Type type)
        => memories[executionId].GetShrinkers(key, type);
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Warnings
    public Warning? StoreWarning(string key, Func<Warning> factory)
        => Current.StoreWarning(key, factory);
    // --------------------------------------------------------------------------
}
