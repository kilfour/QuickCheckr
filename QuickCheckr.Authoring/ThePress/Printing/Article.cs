using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class Article(IRecord record)
{
    public IRecord Record => record;

    private readonly CaseFile? caseFile = (record as CaseFile)!;
    public CaseFile? CaseFile => caseFile;

    private List<CollapsableExecutionDeposition> CollapsableExecutionDepositions =>
        caseFile is null ? [] : caseFile.CollapseExecutions();

    public string VerifyFailed()
    {
        if (caseFile is null) return string.Empty;
        if (caseFile.FailureDeposition is FailedVerificationDeposition verifyingDeposition)
        {
            return verifyingDeposition.FailedExpectation;
        }
        return string.Empty;
    }
    public List<string> FailingExpectationMessages()
    {
        if (caseFile is null) return [];
        return (caseFile.FailureDeposition as FailedExpectationDeposition)?.Messages ?? [];
    }

    public string FailureDescription()
    {
        if (caseFile is null) return string.Empty;
        return caseFile.FailureDeposition.GetFailureDescription();
    }

    public string GetExceptionMessage()
    {
        if (caseFile is null) return string.Empty;
        return (caseFile.FailureDeposition as FailedExceptionDeposition)?.Message ?? string.Empty;
    }

    [StackTraceHidden]
    public ExecutionArticle Execution(int number)
        => new(CollapsableExecutionDepositions[number - 1]!);

    [StackTraceHidden]
    public PassedExpectationArticle PassedExpectation(int number)
    {
        return record is CaseFile
            ? new(caseFile!.PassedExpectations[number - 1])
            : new(((Findings)record).PassedExpectationDepositions[number - 1]);

    }

    public WordCount Total() => new(record);
    public int? Seed() => caseFile?.Seed;
    public int? ShrinkCount => caseFile?.ShrinkCount;
}
