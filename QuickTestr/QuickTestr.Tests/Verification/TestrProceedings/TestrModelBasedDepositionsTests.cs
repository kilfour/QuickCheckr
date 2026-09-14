using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Tests.Verification.TestrProceedings;

public class TestrModelBasedDepositionsTests
{
    private readonly Dossier dossier =
        new(
            FailureInfo: new FailureInfo(FailingExpectation: new ExpectationFailure("Some Invariant", [])),
            RunInfo: new RunInfo(2, 1, 12345678),
            PassedExpectations: new Dictionary<string, int>() { { "Some Invariant", 2 } },
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.Default,
            WarningLevel: WarningLevel.Debug
        );

    private static LinesReader Transcribe(IRecord record)
    {
        var result = TheClerk.Transcribes(record, new ModelClerk().File);
        var reader = LinesReader.FromText(result);
        return reader;
    }

    [Fact]
    public void Full()
    {
        var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("-QTM-Run"))
                .AddInputDeposition(new InputDeposition("PropertyName", 42)
                {
                    Redux = Maybe.Just("1"),
                    Original = Maybe.Just("42")
                }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Falsified after:         2 executions", reader.NextLine());
        Assert.Equal("  Minimal scenario:        1 execution", reader.NextLine());
        Assert.Equal("  Seed:                    12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  1. Run", reader.NextLine());
        Assert.Equal("     42", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  !! Failed: Some Invariant", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    public void PreservesMultiWordOperationLabels()
    {
        var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(
                    new ActionDeposition("-QTM-Delete All By Name")));

        var reader = Transcribe(caseFile);
        reader.NextLine();
        reader.NextLine();
        reader.NextLine();
        reader.NextLine();
        reader.NextLine();

        Assert.Equal(
            "  1. Delete All By Name",
            reader.NextLine());
    }
}
