using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings;

public abstract class DepositionTest
{
    protected virtual bool IgnoreVerdictHeader { get; init; } = true;
    protected virtual bool IgnoreFailingSpec { get; init; } = true;
    protected Dossier Dossier { get; } =
        new Dossier(
                FailingSpec: "Some Invariant",
                Exception: null,
                VerifyingSpec: null,
                OriginalRunExecutionCount: 10,
                ExecutionCount: 4,
                ShrinkCount: 1,
                Seed: 12345678,
                PassedSpecs: new Dictionary<string, int>(),
                UseMemoryForInputReporting: false
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
        if (IgnoreFailingSpec)
            reader.Skip(3);
        Assert.True(reader.EndOfContent());
    }
}