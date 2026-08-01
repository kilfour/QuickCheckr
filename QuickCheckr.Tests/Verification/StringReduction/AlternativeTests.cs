using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class AlternativeTests
{
    [Fact]
    public void UsesTheLeftAlternativeWhenItMatches()
    {
        var selector = Stringr.Once('a').Or(Stringr.Once('b'));
        var state = new SelectrState("a");

        var result = selector(state);

        Assert.Equal("a", result.Value.Text);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void UsesTheRightAlternativeWhenTheLeftDoesNotMatch()
    {
        var selector = Stringr.Once('a').Or(Stringr.Once('b'));
        var state = new SelectrState("b");

        var result = selector(state);

        Assert.Equal("b", result.Value.Text);
        Assert.Equal(1, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void StopsTheAttemptWhenNeitherAlternativeMatches()
    {
        var selector = Stringr.Once('a').Or(Stringr.Once('b'));
        var state = new SelectrState("c");

        var result = selector(state);

        Assert.Null(result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void RestoresTextChangedByAFailedLeftAlternative()
    {
        var left =
            from removed in Stringr.Once('a').Remove()
            from missing in Stringr.Once('x')
            select removed;
        var selector = left.Or(Stringr.Once('a')).Remove().Defined();

        var result = StringReductionTestTools.RunAll(selector, "ab");

        Assert.Equal("b", result);
    }

    [Fact]
    public void RestoresEditOffsetsChangedByAFailedLeftAlternative()
    {
        var left =
            from replaced in Stringr.Once('a').Replace(_ => "zzz")
            from missing in Stringr.Once('x')
            select replaced;
        var selector =
            from chosen in left.Or(Stringr.Once('a')).Editable()
            from replacement in chosen.Replace(_ => "A")
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "ab");

        Assert.Equal("Ab", result);
    }

    [Fact]
    public void RestoresTheChangedFlagAfterAFailedLeftAlternative()
    {
        var left =
            from replaced in Stringr.Once('a').Replace(_ => "A")
            from missing in Stringr.Once('x')
            select replaced;
        var selector = left.Or(Stringr.Once('a'));
        var state = new SelectrState("abc");

        selector(state);

        Assert.False(state.Changed);
    }

    [Fact]
    public void NestedAlternativeDoesNotOverwriteTheOuterRollbackPoint()
    {
        var inner = Stringr.Once('b').Or(Stringr.Once('c'));
        var outerLeft =
            from removed in Stringr.Once('a').Remove()
            from chosen in inner
            from missing in Stringr.Once('x')
            select chosen;
        var selector = outerLeft.Or(Stringr.Once('a'));
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Equal("a", result.Value.Text);
        Assert.Equal("abc", state.Text);
        Assert.True(result.Succeeded);
    }
}
