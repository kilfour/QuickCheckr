using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.Unlabeled;

public class UnlabeledDepostionsTests : DepositionTest
{
    [Fact]
    public void Check()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Making Sure", "42"))
                    .AddInputDeposition(new InputDeposition("Answer", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Universe", "Pooled")));
        var reader = Transcribe(caseFile, false);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Universe    = Pooled", reader.NextLine());
        Assert.Equal("   - Answer      = 42", reader.NextLine());
        Assert.Equal("   - Making Sure = 42", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void CheckAgain()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Making Sure", "42"))
                    .AddInputDeposition(new InputDeposition("Answer", 42) { Original = Maybe.Just("666") })
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Universe", "Pooled")));
        var reader = Transcribe(caseFile, false);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Universe    = Pooled", reader.NextLine());
        Assert.Equal("   - Answer      = 42", reader.NextLine());
        Assert.Equal("                   666", reader.NextLine());
        Assert.Equal("   - Making Sure = 42", reader.NextLine());
        EndOfContent(reader);
    }
}