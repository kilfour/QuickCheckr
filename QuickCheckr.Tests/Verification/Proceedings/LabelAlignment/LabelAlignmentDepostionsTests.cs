using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Tests.Verification.Proceedings.LabelAlignment;

public class LabelAlignmentDepostionsTests : DepositionTest
{
    [Fact]
    public void Check()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("One", "42", false))
                    .AddInputDeposition(new InputDeposition(false, "TwoTwo", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("ThreeThreeThree", "Pooled", false))));
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
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("TwoTwo", "42", false))
                    .AddInputDeposition(new InputDeposition(false, "ThreeThreeThree", 42))
                    .AddPoolTraceDeposition(new PoolTraceDeposition("One", "Pooled", false))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - One             = Pooled", reader.NextLine());
        Assert.Equal("   - ThreeThreeThree = 42", reader.NextLine());
        Assert.Equal("   - TwoTwo          = 42", reader.NextLine());
        EndOfContent(reader);
    }
}