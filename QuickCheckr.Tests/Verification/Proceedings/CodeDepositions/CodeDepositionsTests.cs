using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings.CodeDepositions;

public class CodeDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier)
                .AddExecutionDeposition(new ExecutionDeposition(1))
                .AddCodeDeposition(new CodeDeposition("Just a bit of code;")));
        var result = TheClerk.Transcribes(caseFile);
        var reader = LinesReader.FromText(result);
        // reader.AsAssertsToLogFile();
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    10 executions", reader.NextLine());
        Assert.Equal(" Minimal failing case:    4 executions (after 1 shrink)", reader.NextLine());
        Assert.Equal(" Seed:                    12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        Assert.Equal(" ==================================", reader.NextLine());
        Assert.Equal("  !! Spec Failed: Some Invariant", reader.NextLine());
        Assert.Equal(" ==================================", reader.NextLine());
        Assert.Equal(" Just a bit of code;", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}