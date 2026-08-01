using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class ReplaceTests
{
    [Fact]
    public void ReplacementReceivesTheSelectedText()
    {
        string? selectedText = null;
        var selector = Stringr.Once('a').Replace(text =>
        {
            selectedText = text;
            return text.ToUpperInvariant();
        });
        var state = new SelectrState("abc");

        selector(state);

        Assert.Equal("a", selectedText);
        Assert.Equal("Abc", state.Text);
    }

    [Theory]
    [InlineData("A", "Abc", 1)]
    [InlineData("XYZ", "XYZbc", 3)]
    [InlineData("", "bc", 0)]
    public void ReplacementAdjustsTextAndCursor(string replacement, string expectedText, int expectedIndex)
    {
        var selector = Stringr.Once('a').Replace(_ => replacement);
        var state = new SelectrState("abc");

        selector(state);

        Assert.Equal(expectedText, state.Text);
        Assert.Equal(expectedIndex, state.Index);
        Assert.True(state.Changed);
    }

    [Fact]
    public void RunnerReplacesEveryMatchingCharacter()
    {
        var selector = Stringr.Once('a', 'c').Replace(text => text.ToUpperInvariant()).Defined();

        var candidates = StringReductionTestTools.RunCandidates(selector, "abc");

        Assert.Equal(["Abc", "AbC"], candidates);
    }
}
