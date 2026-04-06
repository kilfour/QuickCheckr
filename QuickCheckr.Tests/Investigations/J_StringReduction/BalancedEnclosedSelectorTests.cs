using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class BalancedEnclosedSelectorTests
{
    private static ISelection? Act(string input)
        => Select.Balanced('(', ')').Enclosed().Extract(input);

    [Fact]
    public void Simple()
    {
        var input = "(1)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(0, slice.Start);
        Assert.Equal(3, slice.Length);
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(4, slice.Start);
        Assert.Equal(8, slice.Length);
    }

    [Fact]
    public void Siblings()
    {
        var input = "(1)(2)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(0, slice.Start);
        Assert.Equal(3, slice.Length);
    }

    [Fact]
    public void SingleNested()
    {
        var input = "((1))";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(0, slice.Start);
        Assert.Equal(5, slice.Length);
    }

    [Fact]
    public void NestedAgain()
    {
        var input = "(1(2)3)";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(0, slice.Start);
        Assert.Equal(7, slice.Length); ;
    }

    [Fact]
    public void Complex()
    {
        var input = "1 + (2 + (3 * 4))";
        var selection = Act(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal(4, slice.Start);
        Assert.Equal(13, slice.Length);
    }

    [Fact]
    public void AfterOther()
    {
        var input = "1 + (2 + a(3 * 4))";
        var selection = Select.OneOf('a').Balanced('(', ')').Enclosed().Extract(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("1 + (2 + )", slice.Remove(input));
    }

    [Fact]
    public void AfterOtherNoSelection()
    {
        var input = "1 + (2 + a(3 * 4))";
        var selection = Select.OneOf('b').Balanced('(', ')').Enclosed().Extract(input);
        Assert.Null(selection);
    }
}