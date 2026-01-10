using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Tests.Verification.Proceedings.Unlabeled;

public class UnlabeledDepostionsTests : DepositionTest
{
    [Fact]
    public void Check()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Making Sure", "42", false))
                    .AddInputDeposition(new InputDeposition(false, "Answer", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Universe", "Pooled", false)));
        var reader = Transcribe(caseFile);
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
                    .AddTraceDeposition(new TraceDeposition("Making Sure", "42", false))
                    .AddInputDeposition(new InputDeposition(false, "Answer", 42) { Original = Maybe.Just("666") })
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Universe", "Pooled", false)));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Universe    = Pooled", reader.NextLine());
        Assert.Equal("   - Answer      = 42", reader.NextLine());
        Assert.Equal("                   666", reader.NextLine());
        Assert.Equal("   - Making Sure = 42", reader.NextLine());
        EndOfContent(reader);
    }
}