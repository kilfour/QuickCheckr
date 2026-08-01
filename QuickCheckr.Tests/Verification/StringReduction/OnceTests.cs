using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class OnceTests
{
    [Fact]
    public void MatchesOneOfTheGivenCharacters()
    {
        var state = new SelectrState("abc");

        var result = Stringr.Once('a', 'b')(state);

        Assert.Equal("a", result.Value.Text);
        Assert.Equal(1, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void EmptyOptionsMatchAnyCharacter()
    {
        var state = new SelectrState("xyz");

        var result = Stringr.Once()(state);

        Assert.Equal("x", result.Value.Text);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void PredicateCanSelectTheCharacter()
    {
        var state = new SelectrState("1a");

        var result = Stringr.Once(char.IsDigit)(state);

        Assert.Equal("1", result.Value.Text);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void MismatchStopsTheAttemptWithoutAdvancing()
    {
        var state = new SelectrState("xyz");

        var result = Stringr.Once('a')(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void EndOfTextStopsTheAttemptWithoutAdvancing()
    {
        var state = new SelectrState(string.Empty);

        var result = Stringr.Once('a')(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
    }

    [Fact]
    public void EndOfTextMarksTheAttemptAsStopped()
    {
        var state = new SelectrState(string.Empty);

        var result = Stringr.Once('a')(state);

        Assert.False(result.Succeeded);
    }
}
