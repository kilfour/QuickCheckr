using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class Article(IRecord record)
{
    private readonly CaseFile caseFile = (record as CaseFile)!;
    public CaseFile CaseFile => caseFile;

    private List<CollapsableExecutionDeposition> CollapsableExecutionDepositions =>
        caseFile.CollapseExecutions();

    public string VerifyFailed()
    {
        if (caseFile.FailureDeposition is FailedVerificationDeposition verifyingDeposition)
        {
            return verifyingDeposition.FailedExpectation;
        }
        return null!;
    }
    public List<string> FailingExpectationMessages()
        => (caseFile.FailureDeposition as FailedExpectationDeposition)?.Messages ?? [];

    public string FailureDescription()
        => caseFile.FailureDeposition.GetFailureDescription();

    public string GetExceptionMessage()
        => (caseFile.FailureDeposition as FailedExceptionDeposition)?.Message ?? string.Empty;

    [StackTraceHidden]
    public ExecutionArticle Execution(int number)
        => new(CollapsableExecutionDepositions[number - 1]!);

    [StackTraceHidden]
    public PassedExpectationArticle PassedExpectation(int number)
        => new(caseFile.PassedExpectationDepositions[number - 1]);

    public WordCount Total() => new(caseFile);
    public int Seed() => caseFile.Seed;
    public int ShrinkCount => caseFile.ShrinkCount;
}
