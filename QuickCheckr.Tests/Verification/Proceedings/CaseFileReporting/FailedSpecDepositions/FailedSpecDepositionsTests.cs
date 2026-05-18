using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.FailedSpecDepositions;

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

    [Fact]
    public void WithMessage()
    {
        var caseFile = CaseFile.From(
            Dossier.FailureInfo with
            {
                FailingExpectation = new ExpectationFailure("Broken", ["It's all broken now.", "Shattered on the floor."])
            }, Dossier.RunInfo);

        var reader = Transcribe(caseFile);
        Assert.Equal(" =================================", reader.NextLine());
        Assert.Equal("  !! Expectation Failed: Broken", reader.NextLine());
        Assert.Equal("       It's all broken now.", reader.NextLine());
        Assert.Equal("       Shattered on the floor.", reader.NextLine());
        Assert.Equal(" =================================", reader.NextLine());
        EndOfContent(reader);
    }
}