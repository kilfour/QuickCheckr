using QuickCheckr.StringReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class ReducerTests
{
    [Fact]
    public async Task IsUnboundedWhenEveryCandidateStillFails()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();

        var reduction = await StringReductionTestTools.ReduceAsync(selector, "abc", _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("c", unbounded.Result);
    }

    [Fact]
    public async Task StopsAtTheLastFailingCandidateAfterWitnessingAPass()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();

        var reduction = await StringReductionTestTools.ReduceAsync(
            selector,
            "abc",
            candidate => ((string)candidate).Length >= 2);

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal("bc", stop.Result);
    }

    [Fact]
    public async Task RejectedCandidateIsRolledBackBeforeSearchingFurtherMatches()
    {
        var selector = Stringr.Once(char.IsWhiteSpace).Remove().Defined();

        var reduction = await StringReductionTestTools.ReduceAsync(
            selector,
            " a  bc ",
            candidate => ((string)candidate).StartsWith(' '));

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal(" abc", stop.Result);
    }

    [Fact]
    public async Task NoMatchingCandidateReturnsTheOriginalInputAsUnbounded()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var reduction = await StringReductionTestTools.ReduceAsync(selector, "bbb", _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("bbb", unbounded.Result);
    }

    [Fact]
    public async Task ReducerFactoryCreatesIndependentSessions()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var first = await StringReductionTestTools.ReduceAsync(selector, "a1", _ => true);
        var second = await StringReductionTestTools.ReduceAsync(selector, "a2", _ => true);

        Assert.Equal("1", Assert.IsType<Reduction.Unbounded>(first).Result);
        Assert.Equal("2", Assert.IsType<Reduction.Unbounded>(second).Result);
    }
}
