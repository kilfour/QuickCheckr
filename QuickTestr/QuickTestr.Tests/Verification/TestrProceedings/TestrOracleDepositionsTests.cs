using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Tests.Verification.TestrProceedings;

public class TestrOracleDepositionsTests
{
    private readonly Dossier dossier =
        new(
            FailureInfo: new FailureInfo(FailingExpectation: new ExpectationFailure("Some Invariant", [])),
            RunInfo: new RunInfo(1, 1, 12345678),
            PassedExpectations: new Dictionary<string, int>() { { "Some Invariant", 2 } },
            UseMemoryForInputReporting: false,
            ReportMode: ReportMode.Default,
            WarningLevel: WarningLevel.Debug
        );

    private static LinesReader Transcribe(IRecord record)
    {
        var result = TheClerk.Transcribes(record, new OracleClerk().File);
        var reader = LinesReader.FromText(result);
        return reader;
    }

    [Fact]
    public void Full()
    {
        var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Run"))
                .AddInputDeposition(new InputDeposition("PropertyName", 42)
                {
                    Redux = Maybe.Just("1"),
                    Original = Maybe.Just("42")
                })
                .AddTraceDeposition(new TraceDeposition("Expected", "42"))
                .AddTraceDeposition(new TraceDeposition("Actual  ", "43"))
                .AddFinalTraceDeposition(new TraceDeposition("Expected", "2"))
                .AddFinalTraceDeposition(new TraceDeposition("Actual  ", "3"))
                );
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Some Invariant", reader.NextLine());
        Assert.Equal("  Seed: 12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Falsified:", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("    Input = 42", reader.NextLine());
        Assert.Equal("      Observed:", reader.NextLine());
        Assert.Equal("        Expected = 42", reader.NextLine());
        Assert.Equal("        Actual   = 43", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("    Redux = 1", reader.NextLine());
        Assert.Equal("      Observed:", reader.NextLine());
        Assert.Equal("        Expected = 2", reader.NextLine());
        Assert.Equal("        Actual   = 3", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("  Original:", reader.NextLine());
        Assert.Equal("    42", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    public void NoRedux_Or_Original()
    {
        var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Run"))
                .AddInputDeposition(new InputDeposition("PropertyName", 42))
                .AddTraceDeposition(new TraceDeposition("Expected", "42"))
                .AddTraceDeposition(new TraceDeposition("Actual  ", "43"))
                .AddFinalTraceDeposition(new TraceDeposition("Expected", "42"))
                .AddFinalTraceDeposition(new TraceDeposition("Actual  ", "43"))
                );
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Some Invariant", reader.NextLine());
        Assert.Equal("  Seed: 12345678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Falsified:", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("    Input = 42", reader.NextLine());
        Assert.Equal("      Observed:", reader.NextLine());
        Assert.Equal("        Expected = 42", reader.NextLine());
        Assert.Equal("        Actual   = 43", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}
