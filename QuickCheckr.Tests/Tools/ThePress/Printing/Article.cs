using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class Article(CaseFile caseFile)
{
    private readonly CaseFile caseFile = caseFile;

    public CaseFile CaseFile { get; } = caseFile;
    public bool VerdictReached() => caseFile.HasVerdict();

    public string VerifyFailed()
    {
        if (caseFile.Verdict?.FailureDeposition is VerifyingDeposition verifyingDeposition)
        {
            return verifyingDeposition.FailedExpectation;
        }
        return null!;
    }

    public string FailureDescription()
        => caseFile.Verdict!.FailureDeposition.GetFailureDescription();

    public string GetExceptionMessage()
        => (caseFile.Verdict!.FailureDeposition as ExceptionDeposition)?.Message ?? string.Empty;

    [StackTraceHidden]
    public ExecutionArticle Execution(int number)
        => new(caseFile.Verdict?.ExecutionDepositions[number - 1]!);

    [StackTraceHidden]
    public PassedExpectationArticle PassedExpectation(int number)
        => new(caseFile.PassedExpectationDepositions[number - 1]);

    public WordCount Total() => new(caseFile);
    public int Seed() => (int)caseFile.Verdict?.Seed!;
    public int ShrinkCount =>
        caseFile.Verdict is null
            ? 0 : caseFile.Verdict!.ShrinkCount;
}
