using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.TraceDepositions;

public class TraceDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something", ["I Traced"])));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something = I Traced", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void UntrimmedLabel()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something   ", ["I Traced"])));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something    = I Traced", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void MultipleLinesAreAlignedWithTheFirstValue()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddTraceDeposition(new TraceDeposition("Something", ["First", "Second", "Third"])));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something =", reader.NextLine());
        Assert.Equal("       First", reader.NextLine());
        Assert.Equal("       Second", reader.NextLine());
        Assert.Equal("       Third", reader.NextLine());
        EndOfContent(reader);
    }
}
