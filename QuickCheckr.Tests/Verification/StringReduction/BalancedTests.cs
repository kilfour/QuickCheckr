using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class BalancedTests
{
    [Fact]
    public void SelectsASimpleBalancedRange()
    {
        var state = new SelectrState("(abc)x");

        var result = Stringr.Balanced('(', ')')(state);

        Assert.Equal("(abc)", result.Value.Text);
        Assert.Equal(5, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void SelectsTheCompleteNestedRange()
    {
        var state = new SelectrState("(a(b)c)x");

        var result = Stringr.Balanced('(', ')')(state);

        Assert.Equal("(a(b)c)", result.Value.Text);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void DetectsNestingImmediatelyAfterTheOpeningCharacter()
    {
        var state = new SelectrState("(())x");

        var result = Stringr.Balanced('(', ')')(state);

        Assert.Equal("(())", result.Value.Text);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void MismatchedOpeningCharacterStopsTheAttempt()
    {
        var state = new SelectrState("abc");

        var result = Stringr.Balanced('(', ')')(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void UnterminatedRangeStopsTheAttempt()
    {
        var state = new SelectrState("(abc");

        var result = Stringr.Balanced('(', ')')(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void EmptyInputStopsTheAttempt()
    {
        var state = new SelectrState(string.Empty);

        var result = Stringr.Balanced('(', ')')(state);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void RunnerCanRemoveNestedDelimitersInSuccessiveCandidates()
    {
        var selector = Stringr.Balanced('(', ')').Replace(text => text[1..^1]).Defined();

        var candidates = StringReductionTestTools.RunCandidates(selector, "1+(2+(3*4)-5)");

        Assert.Equal(
            ["1+2+(3*4)-5", "1+2+3*4-5", "1+2+3*4-5"],
            candidates);
    }
}
