using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class RepetitionTests
{
    [Fact]
    public void AtleastOnceConsumesTheCompleteMatchingRun()
    {
        var state = new SelectrState("aaab");

        var result = Stringr.AtleastOnce('a')(state);

        Assert.Equal("aaa", result.Value.Text);
        Assert.Equal(3, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void AtleastOnceRequiresOneMatch()
    {
        var state = new SelectrState("bbb");

        var result = Stringr.AtleastOnce('a')(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void AtleastOnceSupportsPredicates()
    {
        var state = new SelectrState("123a");

        var result = Stringr.AtleastOnce(char.IsDigit)(state);

        Assert.Equal("123", result.Value.Text);
        Assert.Equal(3, state.Index);
    }

    [Fact]
    public void OptionalConsumesTheCompleteMatchingRun()
    {
        var state = new SelectrState("   x");

        var result = Stringr.Optional(char.IsWhiteSpace)(state);

        Assert.Equal("   ", result.Value.Text);
        Assert.Equal(3, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void OptionalAllowsAnEmptyMatch()
    {
        var state = new SelectrState("bbb");

        var result = Stringr.Optional('a')(state);

        Assert.Equal(string.Empty, result.Value.Text);
        Assert.Equal(0, state.Index);
        Assert.True(result.Succeeded);
    }
}
