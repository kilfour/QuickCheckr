using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.PoolPoolTraceDepositions;

public class PoolTraceDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Something", "I Pooled"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Something = I Pooled", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void UntrimmedLabel()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Something   ", "I Pooled"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Something    = I Pooled", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void AboveInputs()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Making Sure", "42"))
                    .AddInputDeposition(new InputDeposition(true, "Answer", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Universe", "Pooled"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Universe    = Pooled", reader.NextLine());
        Assert.Equal("   - Input: Answer      = 42", reader.NextLine());
        Assert.Equal("   - Trace: Making Sure = 42", reader.NextLine());
        EndOfContent(reader);
    }
}