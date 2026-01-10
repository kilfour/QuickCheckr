using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseSummaryDepositions;

public class CaseSummaryTests
{
    [Fact]
    public void RunTest()
    {
        var summary = CaseSummary.FromCaseFiles([
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 2))
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("Two", 3)),
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 3))
                ]);

        var result = TheClerk.Transcribes(summary);
        var reader = LinesReader.FromText(result);
        Assert.Equal(" 2 Runs", reader.NextLine());
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - One: 5x", reader.NextLine());
        Assert.Equal(" - Two: 3x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}