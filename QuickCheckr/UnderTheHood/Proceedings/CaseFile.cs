

using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Proceedings;


public interface ICaseFileSummary
{
    int OriginalRunExecutionCount { get; }
    int ExecutionCount { get; }
    int ShrinkCount { get; }
    public int ActionCount { get; }
    public int InputCount { get; }
    public int PoolTraceCount { get; }
    public int TraceCount { get; }
    public int WarningCount { get; }
    public int PassedExpectationCount { get; }
    public int InputsNamed<T>(string key, Func<T, int> calculateScore);
    public int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning;
}

public class CaseFile : ICaseFileSummary
{
    public bool HasEvidence { get; init; }
    public FailureDeposition FailureDeposition { get; init; }

    // -----------------------------------------------------------------------
    // ICaseFileSummary
    // --
    public int OriginalRunExecutionCount { get; init; }
    public int ExecutionCount => ExecutionDepositions.Count;
    public int ShrinkCount { get; init; }
    public int ActionCount => ExecutionDepositions.Sum(a => a.ActionDepositions.Count);
    public int InputCount => ExecutionDepositions.Sum(a => a.InputDepositions.Count);
    public int PoolTraceCount => ExecutionDepositions.Sum(a => a.PoolTraceDepositions.Count);
    public int TraceCount => ExecutionDepositions.Sum(a => a.TraceDepositions.Count);
    public int WarningCount => ExecutionDepositions.Sum(a => a.WarningDepositions.Count);
    public int PassedExpectationCount => PassedExpectationDepositions.Count;

    public int InputsNamed<T>(string key, Func<T, int> calculateScore)
    {
        if (!HasEvidence) return int.MaxValue;
        return ExecutionDepositions
            .SelectMany(a => a.InputDepositions)
            .Where(a => a.Label == key)
            .Select(a => a.GetReduxOrValueAsObject())
            .Sum(a => calculateScore((T)a));
    }


    public int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning
    {
        if (!HasEvidence) return int.MaxValue;
        return ExecutionDepositions
            .SelectMany(a => a.WarningDepositions)
            .Where(a => a.Warning is T)
            .Select(a => a.Warning)
            .Sum(a => calculateScore((T)a));
    }
    // -----------------------------------------------------------------------

    public int Seed { get; init; }

    protected CaseFile()
    {
        FailureDeposition = new FailedNoneDeposition();
    }

    private CaseFile(FailureDeposition failureDeposition, RunInfo runInfo)
    {
        FailureDeposition = failureDeposition;
        OriginalRunExecutionCount = runInfo.OriginalRunExecutionCount;
        ShrinkCount = runInfo.ShrinkCount;
        Seed = runInfo.Seed;
        HasEvidence = true;
    }

    public static CaseFile From(FailureInfo failureInfo, RunInfo runInfo)
    {
        return new CaseFile(FailureDeposition.From(failureInfo), runInfo);
    }

    public List<ExecutionDeposition> ExecutionDepositions { get; } = [];
    public CaseFile AddExecutionDeposition(ExecutionDeposition executionDepostion)
    {
        ExecutionDepositions.Add(executionDepostion);
        return this;
    }

    public TestMethodInfoDeposition? TestMethodInfoDeposition { get; private set; }
    public CaseFile AddTestMethodDisposition(TestMethodInfoDeposition testMethodInfoDeposition)
    {
        TestMethodInfoDeposition = testMethodInfoDeposition;
        return this;
    }

    public string GetFailure()
        => FailureDeposition.GetFailureDescription();

    public static CaseFile Empty() => new();

    public List<PassedExpectationDeposition> PassedExpectationDepositions { get; init; } = [];
    public CaseFile AddPassedExpectationDeposition(PassedExpectationDeposition passedExpectationDepositions)
    {
        PassedExpectationDepositions.Add(passedExpectationDepositions);
        return this;
    }
}