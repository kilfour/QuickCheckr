using QuickCheckr;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.FilingCabinet;


public sealed class CaseFile : IRecord
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
    public int? DeliberationScore { get; private set; }
    public int? DeliberationTarget { get; private set; }
    /// <summary>
    /// Gets the values declared by <see cref="Checkr.Distinguish{T}(Func{T})"/>
    /// for this failing case.
    /// </summary>
    public IReadOnlyList<object?> Distinctions => Distinction?.Values ?? [];
    internal DistinctionKey? Distinction { get; private set; }
    internal bool HasDistinction => Distinction is not null;
    internal bool Rejected { get; private set; }

    internal CaseFile AddDeliberation(int score, int? target)
    {
        DeliberationScore = score;
        DeliberationTarget = target;
        return this;
    }

    internal CaseFile AddDistinction(DistinctionKey distinction)
    {
        Distinction = distinction;
        return this;
    }

    internal CaseFile Reject()
    {
        Rejected = true;
        return this;
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

    private CaseFile()
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

    internal static CaseFile From(FailureInfo failureInfo, RunInfo runInfo)
        => new(FailureDeposition.From(failureInfo), runInfo);

    public List<ExecutionDeposition> ExecutionDepositions { get; } = [];
    internal CaseFile AddExecutionDeposition(ExecutionDeposition executionDepostion)
    {
        ExecutionDepositions.Add(executionDepostion);
        return this;
    }

    public TestMethodInfoDeposition? TestMethodInfoDeposition { get; private set; }
    internal CaseFile AddTestMethodDisposition(TestMethodInfoDeposition testMethodInfoDeposition)
    {
        TestMethodInfoDeposition = testMethodInfoDeposition;
        return this;
    }

    public string GetFailure()
        => FailureDeposition.GetFailureDescription();

    internal static CaseFile Empty() => new();
    internal static CaseFile Passed(int seed, List<PassedExpectation> passedExpectations)
    {
        var caseFile = new CaseFile { Seed = seed };
        caseFile.PassedExpectations.AddRange(passedExpectations);
        return caseFile;
    }

    public List<PassedExpectation> PassedExpectations { get; init; } = [];
    internal CaseFile AddPassedExpectationDeposition(PassedExpectation passedExpectationDepositions)
    {
        PassedExpectations.Add(passedExpectationDepositions);
        return this;
    }
}
