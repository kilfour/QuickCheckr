using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.WarningDepositions;

public class WarningDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition("Careful now"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: Careful now", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void BelowTrace()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something   ", "I Traced"))
                    .AddWarningDeposition(new WarningDeposition("Careful now"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something    = I Traced", reader.NextLine());
        Assert.Equal("   - WARNING: Careful now", reader.NextLine());
        EndOfContent(reader);
    }
}