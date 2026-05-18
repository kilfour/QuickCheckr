using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Verification.Proceedings.FindingsReporting;

public class FindingsTests
{
    [Fact]
    public void RunTest()
    {
        var summary = new Findings().Add([
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 2))
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("Two", 3)),
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 3))
                ]);

        var result = TheClerk.Transcribes(summary, The.CourtStyleGuide);
        var reader = LinesReader.FromText(result.PulseToQuickLog());
        Assert.Equal(" 2 Runs", reader.NextLine());
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - One: 5x", reader.NextLine());
        Assert.Equal(" - Two: 3x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}