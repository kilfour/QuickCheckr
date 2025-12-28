using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Tests.Verification.Proceedings;

public class VerifyingDepositionsTests : DepositionTest
{
    public VerifyingDepositionsTests() { IgnoreVerdictHeader = false; IgnoreFailingExpectation = false; }

    [Fact]
    public void One()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(new Dossier(
                FailingExpectation: null,
                Exception: null,
                Verification: "Some Invariant",
                OriginalRunExecutionCount: 10,
                ExecutionCount: 4,
                ShrinkCount: 0,
                Seed: 12345678,
                PassedExpectations: new Dictionary<string, int>(),
                UseMemoryForInputReporting: false,
                ReportWarnings: true,
                ReportStackTrace: false
            )));

        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    10 executions", reader.NextLine());
        Assert.Equal(" Seed:                    12345678", reader.NextLine());
        Assert.Equal(" ==============================================", reader.NextLine());
        Assert.Equal("  !!  The Verifier Disagrees: Some Invariant", reader.NextLine());
        Assert.Equal(" ==============================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}