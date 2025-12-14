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
        if (caseFile.Verdict!.FailureDeposition is VerifyingDeposition assayerDeposition)
        {
            return assayerDeposition.FailedExpectation;
        }
        return null!;
    }

    public string FailedExpectation()
    {
        if (caseFile.Verdict!.FailureDeposition is FailedExpectationDeposition failedSpecDeposition)
        {
            return failedSpecDeposition.FailedExpectation;
        }
        return null!;
    }

    public Exception Exception()
    {
        if (caseFile.Verdict!.FailureDeposition is ExceptionDeposition exceptionDeposition)
        {
            return exceptionDeposition.Exception;
        }
        return null!;
    }

    [StackTraceHidden]
    public ExecutionArticle Execution(int number)
        => new(caseFile.Verdict!.ExecutionDepositions[number - 1]);

    [StackTraceHidden]
    public PassedSpecArticle PassedSpec(int number)
        => new(caseFile.PassedExpectationDepositions[number - 1]);

    public WordCount Total() => new(caseFile);
    public int Seed() => caseFile.Verdict!.Seed;
    public int ShrinkCount => caseFile.Verdict!.ShrinkCount;
}
