using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class CompositionTests
{
    [Fact]
    public void SelectProjectsTheSelectedValue()
    {
        var selector = Stringr.Once('a').Select(selection => selection.Text.ToUpperInvariant());
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Equal("A", result.Value);
        Assert.Equal(1, state.Index);
    }

    [Fact]
    public void SelectManyRunsSelectorsSequentially()
    {
        var selector =
            from first in Stringr.Once('a')
            from second in Stringr.Once('b')
            select first.Text + second.Text;
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Equal("ab", result.Value);
        Assert.Equal(2, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void SelectManyDoesNotRunTheSecondSelectorWhenTheFirstFails()
    {
        var secondWasRun = false;
        SelectrOf<ITextSelection> second = state =>
        {
            secondWasRun = true;
            return Stringr.Once('b')(state);
        };
        var selector = Stringr.Once('a').SelectMany(_ => second);
        var state = new SelectrState("bbb");

        var result = selector(state);

        Assert.False(secondWasRun);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void QueryCanUseAnEarlierSelectionsTextInALaterEdit()
    {
        var selector =
            from first in Stringr.Once('a')
            from second in Stringr.Once('b').Replace(_ => first.Text.ToUpperInvariant())
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "ab");

        Assert.Equal("aA", result);
    }

    [Fact]
    public void DefinedAdaptsASelectorForExecution()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var result = StringReductionTestTools.RunAll(selector, "a");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SelectShortCircuitsWhenItsSourceFails()
    {
        var projectorWasRun = false;
        var selector = Stringr.Once('a').Select(selection =>
        {
            projectorWasRun = true;
            return selection.Text;
        });
        var state = new SelectrState("bbb");

        var result = selector(state);

        Assert.False(projectorWasRun);
        Assert.False(result.Succeeded);
    }
}
