using QuickCheckr.StringReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class ReducerTests
{
    [Fact]
    public void IsUnboundedWhenEveryCandidateStillFails()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();

        var reduction = StringReductionTestTools.Reduce(selector, "abc", _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("c", unbounded.Result);
    }

    [Fact]
    public void StopsAtTheLastFailingCandidateAfterWitnessingAPass()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();

        var reduction = StringReductionTestTools.Reduce(
            selector,
            "abc",
            candidate => ((string)candidate).Length >= 2);

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal("bc", stop.Result);
    }

    [Fact]
    public void RejectedCandidateIsRolledBackBeforeSearchingFurtherMatches()
    {
        var selector = Stringr.Once(char.IsWhiteSpace).Remove().Defined();

        var reduction = StringReductionTestTools.Reduce(
            selector,
            " a  bc ",
            candidate => ((string)candidate).StartsWith(' '));

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal(" abc", stop.Result);
    }

    [Fact]
    public void NoMatchingCandidateReturnsTheOriginalInputAsUnbounded()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var reduction = StringReductionTestTools.Reduce(selector, "bbb", _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("bbb", unbounded.Result);
    }

    [Fact]
    public void ReducerFactoryCreatesIndependentSessions()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var first = StringReductionTestTools.Reduce(selector, "a1", _ => true);
        var second = StringReductionTestTools.Reduce(selector, "a2", _ => true);

        Assert.Equal("1", Assert.IsType<Reduction.Unbounded>(first).Result);
        Assert.Equal("2", Assert.IsType<Reduction.Unbounded>(second).Result);
    }
}
