using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.ActionDepositions;

public class TraceDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something", "I Traced"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something = I Traced", reader.NextLine());
        EndOfContent(reader);
    }
}