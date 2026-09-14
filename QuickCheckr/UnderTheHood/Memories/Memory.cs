using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Warnings;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Memories;


internal sealed class Memory
{
    public KeyGuardian KeyGuardian = new();
    public int StartNewExecution()
    {
        KeyGuardian = new();
        return ShortTermMemory.StartNewExecution();
    }

    public void GotoExecution(int executionNumber)
    {
        KeyGuardian = new();
        ShortTermMemory.GotoExecution(executionNumber);
    }

    public int NextExecution()
    {
        KeyGuardian = new();
        return ShortTermMemory.NextExecution();
    }

    public void DisposeStash()
        => LongTermMemory.DisposeStash();

    public int Reset()
    {
        KeyGuardian = new();
        LongTermMemory.Reset();
        return ShortTermMemory.Reset();
    }

    public IReadOnlyList<int> Numbers
        => ShortTermMemory.Numbers;
    public int Count()
        => ShortTermMemory.Count();
    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => ShortTermMemory.Entries();
    public void Remove(int executionNumber)
        => ShortTermMemory.Remove(executionNumber);
    public IDisposable ScopedExecutionRemoval(int executionNumber)
        => ShortTermMemory.ScopedExecutionRemoval(executionNumber);
    public IDisposable ScopedSwap(int executionId, string key, object value)
        => ShortTermMemory.ScopedSwap(executionId, key, value);

    public IDisposable NestedValue(int executionId, MemoryLens swapper)
        => ShortTermMemory.NestedValue(executionId, swapper);
    public void Commit(int executionId, string key, object value)
        => ShortTermMemory.Commit(executionId, key, value);
    public void Testify(int executionId, string key, object value)
        => ShortTermMemory.Testify(executionId, key, value);
    public IDisposable WithTestimony(int executionId, string key, object value)
        => ShortTermMemory.WithTestimony(executionId, key, value);

    // ==========================================================================
    // Long Term
    // == 
    public LongTermMemory LongTermMemory { get; } = new();
    // --------------------------------------------------------------------------
    // Variables
    public void StoreLongTermVariable(string key, object value)
        => LongTermMemory.StoreLongTermVariable(KeyGuardian.LongTermVariables.Check(key), value);
    public T GetLongTermVariable<T>(string key, T defaultValue)
        => LongTermMemory.GetLongTermVariable(key, defaultValue);
    // --------------------------------------------------------------------------
    // Stashed
    public T StoreStashed<T>(string key, Func<T> factory)
        => LongTermMemory.StoreStashed(KeyGuardian.Stash.Check(key), factory);
    public T GetStashed<T>(string key)
        => LongTermMemory.GetStashed<T>(key);
    // --------------------------------------------------------------------------
    // Pool
    public IEnumerable<T> GetPool<T>()
        => LongTermMemory.GetPool<T>();
    public Dictionary<int, T> GetPoolDictionary<T>()
        => LongTermMemory.GetPoolDictionary<T>();
    public IEnumerable<int> GetPoolIds<T>()
        => LongTermMemory.GetPoolIds<T>();
    public IEnumerable<int> GetPoolIds<T>(Func<T, bool> predicate)
        => LongTermMemory.GetPoolIds(predicate);
    public (bool, T?) GetFromPool<T>(int id)
        => LongTermMemory.GetFromPool<T>(id);
    public T StoreInPool<T>(int id, T value)
        => LongTermMemory.StoreInPool(id, value);
    public void ClearPool<T>()
        => LongTermMemory.ClearPool<T>();
    // --------------------------------------------------------------------------


    // ==========================================================================
    // Short Term
    // == 
    public ShortTermMemory ShortTermMemory { get; } = new();
    public bool IsConfigRun { get; set; } = false;
    // --------------------------------------------------------------------------
    // Case Selection
    private bool hasDeliberation;
    private bool hasDistinction;
    private bool hasCaseRejection;
    internal bool Deliberating { get; set; }
    internal bool Distinguishing { get; set; }
    internal bool RejectingCases { get; set; }
    internal bool HasDeliberation => hasDeliberation;
    internal bool HasDistinction => hasDistinction;
    internal bool HasCaseRejection => hasCaseRejection;
    internal void StoreDeliberation(Func<int> score)
    {
        hasDeliberation = true;
        if (Deliberating)
            ShortTermMemory.StoreDeliberation(score());
    }
    internal void StoreRunDeliberation(Func<CaseFile, int> score, bool isLastExecution)
    {
        hasDeliberation = true;
        if (Deliberating && isLastExecution)
            ShortTermMemory.StoreRunDeliberation(score);
    }
    internal void StoreDeliberationTarget(int target, bool isLastExecution)
    {
        if (Deliberating && isLastExecution)
            ShortTermMemory.StoreDeliberationTarget(target);
    }
    internal void StoreDistinction(Func<object?> distinction)
    {
        hasDistinction = true;
        if (Distinguishing)
            ShortTermMemory.StoreDistinction(distinction());
    }
    internal void StoreCaseRejection(Func<bool> reject)
    {
        hasCaseRejection = true;
        if (RejectingCases)
            ShortTermMemory.StoreCaseRejection(reject());
    }
    internal void ClearCaseSelection()
        => ShortTermMemory.ClearCaseSelection();
    internal CaseSelectionResult? GetCaseSelection()
        => ShortTermMemory.GetCaseSelection();
    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
    {
        var value = ShortTermMemory.StoreInput(KeyGuardian.Inputs.Check(key), factory);
        if (IsConfigRun)
        {
            ShortTermMemory.StoreOriginal(key, value);
        }
        return value;
    }
    public object? GetCurrentInputValue(int executionId, string key)
        => ShortTermMemory.GetCurrentInputValue(executionId, key);
    // Input Showrs
    public void StoreInputShowr(Func<ManOfWealthAndTaste> showr)
    {
        if (IsConfigRun)
            ShortTermMemory.StoreInputShowr(showr);
    }
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => ShortTermMemory.StoreAction(KeyGuardian.Acts.Check(key));
    public void StoreShrinkableAction(string key)
    {
        var checkedKey = KeyGuardian.Acts.Check(key);
        ShortTermMemory.StoreAction(checkedKey);
        ShortTermMemory.StoreShrinkableAction(checkedKey);
    }
    public IReadOnlyList<string> GetActions() => ShortTermMemory.GetActions();
    // --------------------------------------------------------------------------
    // Traces
    public bool Tracing { get; set; } = false;
    public bool FinalTrace { get; set; } = false;
    public void StoreTrace(string key, Func<string> factory)
    {
        if (Tracing)
            ShortTermMemory.StoreTrace(KeyGuardian.Traces.Check(key), factory);
        if (FinalTrace)
            ShortTermMemory.StoreFinalTrace(KeyGuardian.Traces.Check(key), factory);
    }
    // --------------------------------------------------------------------------
    // Traces
    public void StorePoolTrace(string key, Func<string> factory)
    {
        if (Tracing)
            ShortTermMemory.StorePoolTrace(KeyGuardian.PoolTraces.Check(key), factory);
    }
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => ShortTermMemory.StoreVariable(KeyGuardian.Variables.Check(key), factory);
    // --------------------------------------------------------------------------

    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinkers(string key, Shrinker[] shrinkers)
        => ShortTermMemory.StoreShrinkers(key, shrinkers);
    public Shrinker[]? GetShrinkers(int executionId, string key, Type type)
        => ShortTermMemory.GetShrinkers(executionId, key, type);
    public bool HasClassifier(ShrinkingInfo info)
    {
        var shrinkers = GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
        {
            var shrinker = shrinkers.Where(a => a.Classifiers.Length != 0).FirstOrDefault(x => x.Condition(info));
            if (shrinker is not null)
                return true;
        }
        return false;
    }

    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Warnings
    public Warning? StoreWarning(string key, Func<Warning> factory)
        => ShortTermMemory.StoreWarning(key, factory);
}
