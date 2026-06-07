using QuickCheckr;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.FilingCabinet;


// public interface ICaseFileSummary
// {
//     int OriginalRunExecutionCount { get; }
//     int ExecutionCount { get; }
//     int ShrinkCount { get; }
//     public int ActionCount { get; }
//     public int InputCount { get; }
//     public int PoolTraceCount { get; }
//     public int TraceCount { get; }
//     public int WarningCount { get; }
//     public int PassedExpectationCount { get; }
//     public T GetInput<T>(string key);
//     public int InputsNamed<T>(string key, Func<T, int> calculateScore);
//     public int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning;
// }

public class CaseFile : IRecord
{
    public bool HasEvidence => FailureDeposition is not FailedNoneDeposition;
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
    public int PassedExpectationCount => PassedExpectations.Count;


    public T GetInput<T>(string key)
    {
        return (T)ExecutionDepositions
            .SelectMany(a => a.InputDepositions)
            .Where(a => a.Label == key)
            .Select(a => a.GetReduxOrValueAsObject())
            .First();
    }

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
    }

    public static CaseFile From(FailureInfo failureInfo, RunInfo runInfo)
        => new(FailureDeposition.From(failureInfo), runInfo);

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
    public static CaseFile Passed(int seed, List<PassedExpectation> passedExpectations)
    {
        var caseFile = new CaseFile { Seed = seed };
        caseFile.PassedExpectations.AddRange(passedExpectations);
        return caseFile;
    }

    public List<PassedExpectation> PassedExpectations { get; init; } = [];
    public CaseFile AddPassedExpectationDeposition(PassedExpectation passedExpectationDepositions)
    {
        PassedExpectations.Add(passedExpectationDepositions);
        return this;
    }
}