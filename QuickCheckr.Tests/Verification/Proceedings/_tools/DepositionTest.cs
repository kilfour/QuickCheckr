using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings;

public abstract class DepositionTest
{
    protected virtual bool IgnoreVerdictHeader { get; init; } = true;
    protected virtual bool IgnoreFailingExpectation { get; init; } = true;
    protected Dossier Dossier { get; } =
        new Dossier(
                FailureInfo: new FailureInfo(FailingExpectation: new ExpectationFailure("Some Invariant", [])),
                RunInfo: new RunInfo(10, 1, 12345678),
                PassedExpectations: new Dictionary<string, int>(),
                UseMemoryForInputReporting: false,
                ReportMode: ReportMode.Default,
                WarningLevel: WarningLevel.Debug
            );

    protected LinesReader Transcribe(CaseFile caseFile, bool includeLabels = true)
    {
        var clerk = includeLabels ? CourtClerk.Default().WithLabels() : CourtClerk.Default();
        var result = TheClerk.Transcribes(caseFile, clerk.File);
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