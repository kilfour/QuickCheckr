using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings;

public class FailedExpectationDepositionsTests : DepositionTest
{
    public FailedExpectationDepositionsTests() { IgnoreFailingExpectation = false; }

    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo);

        var reader = Transcribe(caseFile);
        Assert.Equal(" =========================================", reader.NextLine());
        Assert.Equal("  !! Expectation Failed: Some Invariant", reader.NextLine());
        Assert.Equal(" =========================================", reader.NextLine());
        EndOfContent(reader);
    }
}