using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class BalancedDelimitersSelectorTests
{
    private static ISelection? Act(string input)
        => Select.Balanced('(', ')').Delimiters().Extract(input);

    [Fact]
    public void Simple()
    {
        var input = "(1)";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([0, 2], indexSet.Indices);
    }

    [Fact]
    public void Chain()
    {
        var input = "(1 + 2) + (1 + 2) + (1 + 2)";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal("1 + 2 + (1 + 2) + (1 + 2)", indexSet.Remove(input));
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([4, 11], indexSet.Indices);
    }

    [Fact]
    public void Siblings()
    {
        var input = "(1)(2)";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([0, 2], indexSet.Indices);

    }

    [Fact]
    public void SingleNested()
    {
        var input = "((1))";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([0, 4], indexSet.Indices);
    }

    [Fact]
    public void NestedAgain()
    {
        var input = "(1(2)3)";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([0, 6], indexSet.Indices);
    }

    [Fact]
    public void Complex()
    {
        var input = "1 + (2 + (3 * 4))";
        var selection = Act(input);
        var indexSet = Assert.IsType<IndexSet>(selection);
        Assert.Equal([4, 16], indexSet.Indices);
    }
}