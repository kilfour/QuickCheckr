using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class Article(CaseFile caseFile)
{
    private readonly CaseFile caseFile = caseFile;
    public CaseFile CaseFile { get; } = caseFile;

    private readonly List<CollapsableExecutionDeposition> collapsableExecutionDepositions =
        caseFile.CollapseExecutions();

    public string VerifyFailed()
    {
        if (caseFile.FailureDeposition is FailedVerificationDeposition verifyingDeposition)
        {
            return verifyingDeposition.FailedExpectation;
        }
        return null!;
    }

    public string FailureDescription()
        => caseFile.FailureDeposition.GetFailureDescription();

    public string GetExceptionMessage()
        => (caseFile.FailureDeposition as FailedExceptionDeposition)?.Message ?? string.Empty;

    [StackTraceHidden]
    public ExecutionArticle Execution(int number)
        => new(collapsableExecutionDepositions[number - 1]!);

    [StackTraceHidden]
    public UncheckedExpectationArticle UncheckedExpectation(int number)
        => new(caseFile.UncheckedExpectationDepositions[number - 1]);

    [StackTraceHidden]
    public PassedExpectationArticle PassedExpectation(int number)
        => new(caseFile.PassedExpectationDepositions[number - 1]);

    public WordCount Total() => new(caseFile);
    public int Seed() => caseFile.Seed;
    public int ShrinkCount => caseFile.ShrinkCount;
}
