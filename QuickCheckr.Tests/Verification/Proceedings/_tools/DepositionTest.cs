using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings;

public abstract class DepositionTest
{
    protected virtual bool IgnoreVerdictHeader { get; init; } = true;
    protected virtual bool IgnoreFailingExpectation { get; init; } = true;
    protected Dossier Dossier { get; } =
        new Dossier(
                FailingExpectation: "Some Invariant",
                Exception: null,
                Verification: null,
                OriginalRunExecutionCount: 10,
                ExecutionCount: 4,
                ShrinkCount: 1,
                Seed: 12345678,
                PassedExpectations: new Dictionary<string, int>(),
                UseMemoryForInputReporting: false,
                ReportWarnings: true
            );

    protected LinesReader Transcribe(CaseFile caseFile)
    {
        var result = TheClerk.Transcribes(caseFile);
        var reader = LinesReader.FromText(result);
        if (IgnoreVerdictHeader)
            reader.Skip(4);
        return reader;

    }

    [StackTraceHidden]
    protected void EndOfContent(LinesReader reader)
    {
        if (IgnoreFailingExpectation)
            reader.Skip(3);
        Assert.True(reader.EndOfContent());
    }
}