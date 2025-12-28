using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings;

public class ExceptionDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier with { FailingExpectation = null, Exception = new Exception("BOOM") }));

        var reader = Transcribe(caseFile);
        reader.AsAssertsToLogFile();
        Assert.Equal(" ===========================================================================", reader.NextLine());
        Assert.Equal("  !! Exception Thrown: System.Exception: BOOM", reader.NextLine());
        Assert.Equal(" ===========================================================================", reader.NextLine());
        EndOfContent(reader);
    }
}