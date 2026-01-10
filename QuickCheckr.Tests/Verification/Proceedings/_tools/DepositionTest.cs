using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings;

public abstract class DepositionTest
{
    protected virtual bool IgnoreVerdictHeader { get; init; } = true;
    protected virtual bool IgnoreFailingExpectation { get; init; } = true;
    protected Dossier Dossier { get; } =
        new Dossier(
                FailureInfo: new FailureInfo(FailingExpectation: "Some Invariant"),
                RunInfo: new RunInfo(10, 4, 1, 12345678),
                PassedExpectations: new Dictionary<string, int>(),
                UseMemoryForInputReporting: false,
                ReportMode: ReportMode.Default & ~ReportMode.StackTrace
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