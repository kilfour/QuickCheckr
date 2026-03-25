using QuickCheckr.Invariants;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.TestrProceedings;

public class TestrDepositionsTests
{
    private readonly Dossier dossier =
        new(
            FailureInfo: new FailureInfo(FailingExpectation: "Some Invariant"),
            RunInfo: new RunInfo(1, 1, 12345678),
            PassedExpectations: new Dictionary<string, int>() { { "Some Invariant", 2 } },
            UncheckedExpectations: [],
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.Default & ~ReportMode.StackTrace
        );

    private static LinesReader Transcribe(CaseFile caseFile)
    {
        var result = TheClerk.Transcribes(caseFile, TheTestr.StyleGuide);
        var reader = LinesReader.FromText(result);
        return reader;
    }

    [Fact]
    public void Full()
    {
        var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Run"))
                .AddInputDeposition(new InputDeposition(true, "PropertyName", 42)
                {
                    Redux = Maybe.Just("1"),
                    Original = Maybe.Just("42")
                }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Some Invariant", reader.NextLine());
        Assert.Equal("  Seed: 12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Falsified:", reader.NextLine());
        Assert.Equal("    Input = 42", reader.NextLine());
        Assert.Equal("    Redux = 1", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("  Original:", reader.NextLine());
        Assert.Equal("    42", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}