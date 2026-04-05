using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class ReduceTermsTests
{
    private static ISelection? Act(string input)
        => Select
            .Balanced('(', ')').Enclosed().While(char.IsWhiteSpace).OneOf('+')
            .Or(Select.While(char.IsDigit).While(char.IsWhiteSpace).OneOf('+'))
            .Extract(input);

    [Fact]
    public void Single()
    {
        var input = "1";
        var selection = Act(input);
        Assert.Null(selection);
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(" ( 2 + 3)", slice.Remove(input));
    }

    [Fact]
    public void OneTwoThree_Again()
    {
        var input = "(1 + 2) + 3";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(" 3", slice.Remove(input));
    }

    [Fact]
    public void Multiply_Involved()
    {
        var input = "1 * 2 + 3";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("1 *  3", slice.Remove(input));
    }

    [Fact]
    public void RightToLeft()
    {
        var input = "((1 + 2) + 3) * 4";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("(( 2) + 3) * 4", slice.Remove(input));
    }

    [Fact]
    public void NoPlusNoReduction()
    {
        var input = "1 * 2";
        var selection = Act(input);
        Assert.Null(selection);
    }
}