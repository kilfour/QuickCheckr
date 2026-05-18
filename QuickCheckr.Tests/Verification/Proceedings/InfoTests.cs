using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings;

public class InfoTests : DepositionTest
{
    public InfoTests() { IgnoreVerdictHeader = false; }
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    10 executions", reader.NextLine());
        Assert.Equal(" Minimal failing case:    1 execution (after 1 shrink)", reader.NextLine());
        Assert.Equal(" Seed:                    12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        Assert.Equal(" =========================================", reader.NextLine());
        Assert.Equal("  !! Expectation Failed: Some Invariant", reader.NextLine());
        Assert.Equal(" =========================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}