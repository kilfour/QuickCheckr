

using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

namespace QuickCheckr.UnderTheHood.Proceedings;

public class CaseFile
{
    public bool HasEvidence { get; init; }
    public FailureDeposition FailureDeposition { get; init; }

    public int OriginalRunExecutionCount { get; init; }
    public int ExecutionCount { get; init; }
    public int ShrinkCount { get; init; }
    public int Seed { get; init; }

    protected CaseFile()
    {
        FailureDeposition = new FailedNoneDeposition();
    }

    private CaseFile(FailureDeposition failureDeposition, RunInfo runInfo)
    {
        FailureDeposition = failureDeposition;
        OriginalRunExecutionCount = runInfo.OriginalRunExecutionCount;
        ExecutionCount = runInfo.ExecutionCount;
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

    public List<CollapsableExecutionDeposition> GetCollapsableExecutionDepositions()
    {
        CollapsableExecutionDeposition? collapsableExecutionDeposition = null;
        var result = new List<CollapsableExecutionDeposition>();
        foreach (var executionDeposition in ExecutionDepositions)
        {
            if (collapsableExecutionDeposition == null)
            {
                collapsableExecutionDeposition = new CollapsableExecutionDeposition(executionDeposition);
                result.Add(collapsableExecutionDeposition);
            }
            else
            {
                if (collapsableExecutionDeposition.Collapsed(executionDeposition))
                    continue;
                collapsableExecutionDeposition = new CollapsableExecutionDeposition(executionDeposition);
                result.Add(collapsableExecutionDeposition);
            }
        }
        return result;
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

    public int InputsNamed(string key, Func<string, int> scorer)
    {
        if (!HasEvidence) return int.MaxValue;
        return ExecutionDepositions
            .SelectMany(a => a.InputDepositions)
            .Where(a => a.Label == key)
            .Select(a => a.GetReduxOrValue())
            .Sum(a => scorer(a));
    }
}