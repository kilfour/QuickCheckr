using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.PoolPoolTraceDepositions;

public class PoolPoolTraceDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Something", "I Traced"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Something = I Traced", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void UntrimmedLabel()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Something   ", "I Traced"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Something    = I Traced", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void AboveTrace()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something", "I Traced"))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("Something", "I Traced"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   -  Pool: Something = I Traced", reader.NextLine());
        Assert.Equal("   - Trace: Something = I Traced", reader.NextLine());
        EndOfContent(reader);
    }
}