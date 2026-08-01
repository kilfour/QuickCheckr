using QuickCheckr.StringReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class CompoundSelectionTests
{
    [Fact]
    public void SearchContinuesPastAnIncompleteCompoundMatch()
    {
        var selector =
            from first in Stringr.Once('a', 'b').Remove()
            from second in Stringr.Once('a', 'b').Remove()
            select Selection.Defined;

        var candidate = selector.Run("azabc").Next();

        Assert.Equal("azc", candidate);
    }

    [Fact]
    public void RepeatedCompoundMatchesProduceSuccessiveCandidates()
    {
        var selector =
            from selected in Stringr.Once('a', 'b').Remove()
            from replaced in Stringr.Once('c', 'd').Replace(_ => selected.Text.ToUpperInvariant())
            select Selection.Defined;

        var run = selector.Run("--ac--bd--");

        Assert.Equal("--A--bd--", run.Next());
        Assert.Equal("--A--B--", run.Next());
    }

    [Fact]
    public void CompoundSelectorRemovesPlainParenthesesSuccessively()
    {
        var selector =
            from left in Stringr.Once('(').Remove()
            from contents in Stringr.Until(')')
            from right in Stringr.Once(')').Remove()
            select Selection.Defined;

        var candidates = StringReductionTestTools.RunCandidates(selector, "1+(2+(3*4))");

        Assert.Equal(["1+2+(3*4)", "1+2+3*4"], candidates);
    }

    [Theory]
    [InlineData("39+2", "+")]
    [InlineData("1+(2 + 3)+4", "++")]
    public void AlternativeCanRemoveNumbersOrBalancedRanges(string input, string expected)
    {
        var number = Stringr.AtleastOnce(char.IsDigit);
        var enclosed = Stringr.Balanced('(', ')');
        var selector = number.Or(enclosed).Remove().Defined();

        var result = StringReductionTestTools.RunAll(selector, input);

        Assert.Equal(expected, result);
    }
}
