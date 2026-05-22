using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.LabelAlignment;

public class LabelAlignmentDepostionsTests : DepositionTest
{
    [Fact]
    public void Check()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("One", "42"))
                    .AddInputDeposition(new InputDeposition("TwoTwo", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("ThreeThreeThree", "Pooled")));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - ThreeThreeThree = Pooled", reader.NextLine());
        Assert.Equal("   - TwoTwo          = 42", reader.NextLine());
        Assert.Equal("   - One             = 42", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void CheckAgain()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("TwoTwo", "42"))
                    .AddInputDeposition(new InputDeposition("ThreeThreeThree", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("One", "Pooled")));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - One             = Pooled", reader.NextLine());
        Assert.Equal("   - ThreeThreeThree = 42", reader.NextLine());
        Assert.Equal("   - TwoTwo          = 42", reader.NextLine());
        EndOfContent(reader);
    }
}