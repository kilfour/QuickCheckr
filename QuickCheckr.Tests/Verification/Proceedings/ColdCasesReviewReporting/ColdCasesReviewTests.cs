using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Verification.Proceedings.ColdCasesReviewReporting;

public class ColdCasesReviewTests
{
    [Fact]
    public void RunTest()
    {
        var summary = new ColdCasesReview().Add([
            CaseFile.Passed(584, [
                new PassedExpectation("One", 2),
                new PassedExpectation("Two", 3)]),
            CaseFile.Passed(6889, [new PassedExpectation("One", 3)])
                ]);

        var result = TheClerk.Transcribes(summary, CourtClerk.Default().File);
        var reader = LinesReader.FromText(result);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" 2 Runs", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Seed:                    584", reader.NextLine());
        Assert.Equal(" Result:                  Passed", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Seed:                    6889", reader.NextLine());
        Assert.Equal(" Result:                  Passed", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - One: 5x", reader.NextLine());
        Assert.Equal(" - Two: 3x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}