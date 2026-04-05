using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class WhileSelectorTests
{
    private static ISelection? Act(string input)
        => Select.While(char.IsDigit).Extract(input);

    [Fact]
    public void Null()
    {
        var input = "a";
        var selection = Act(input);
        Assert.Null(selection);
    }

    [Fact]
    public void Single()
    {
        var input = "1";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("", slice.Remove(input));
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(" + ( 2 + 3)", slice.Remove(input));
    }

    [Fact]
    public void Parenthesis()
    {
        var input = "(145r + 2)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("(r + 2)", slice.Remove(input));
    }

    [Fact]
    public void RightToLeft()
    {
        var input = "((123 + 2) + 3) * 4";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("(( + 2) + 3) * 4", slice.Remove(input));
    }

    [Fact]
    public void WhileWhile()
    {
        var input = "123 4aa";
        var selection = Select.While(char.IsDigit).While(char.IsWhiteSpace).Extract(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("4aa", slice.Remove(input));
    }

    [Fact]
    public void ZeroLengthLeftWithoutMatch_ReturnsNull()
    {
        var input = "abc";
        var selection = new SkipWhile(char.IsLetter).While(char.IsWhiteSpace).Extract(input);
        Assert.Null(selection);
    }
}
