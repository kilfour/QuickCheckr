using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings;

public class VerifyingDepositionsTests : DepositionTest
{
    public VerifyingDepositionsTests() { IgnoreVerdictHeader = false; IgnoreFailingExpectation = false; }

    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(new FailureInfo(Verification: "Some Invariant"), Dossier.RunInfo with { ShrinkCount = 0 });
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    10 executions", reader.NextLine());
        Assert.Equal(" Seed:                    12345678", reader.NextLine());
        Assert.Equal(" =============================================", reader.NextLine());
        Assert.Equal("  !! The Verifier Disagrees: Some Invariant", reader.NextLine());
        Assert.Equal(" =============================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}