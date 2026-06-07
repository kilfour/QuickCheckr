using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings.InquiryReporting;

public class InquiryTests
{
    [Fact]
    public void RunTest()
    {
        var summary = new Inquiry().Add([
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectation("One", 2))
                .AddPassedExpectationDeposition(new PassedExpectation("Two", 3)),
            CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectation("One", 3))
                ]);

        var result = TheClerk.Transcribes(summary, CourtClerk.Default().File);
        var reader = LinesReader.FromText(result);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" 2 Total Runs", reader.NextLine());
        Assert.Equal(" 0 Failed Run", reader.NextLine());
        Assert.Equal(" 0 Distinct Failure", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal("", reader.NextLine());
        Assert.Equal(" ============================================================", reader.NextLine());
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - One: 5x", reader.NextLine());
        Assert.Equal(" - Two: 3x", reader.NextLine());
        Assert.Equal(" ============================================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}