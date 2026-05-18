using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.TestMethodInfoDepositions;

public class TestMethodInfoDepositionTests : DepositionTest
{
    public TestMethodInfoDepositionTests()
    {
        IgnoreVerdictHeader = false;
    }

    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1))
                .AddExecutionDeposition(new ExecutionDeposition(2))
                .AddTestMethodDisposition(new TestMethodInfoDeposition("method name", "source/file.cs", 42));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Test:                    method name", reader.NextLine());
        Assert.Equal(" Location:                source/file.cs:42:1", reader.NextLine());
        Assert.Equal(" Original failing run:    10 executions", reader.NextLine());
        Assert.Equal(" Minimal failing case:    2 executions (after 1 shrink)", reader.NextLine());
        Assert.Equal(" Seed:                    12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (2 Times)", reader.NextLine());
        Assert.Equal(" =========================================", reader.NextLine());
        Assert.Equal("  !! Expectation Failed: Some Invariant", reader.NextLine());
        Assert.Equal(" =========================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}