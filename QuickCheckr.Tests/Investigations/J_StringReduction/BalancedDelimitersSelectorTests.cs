using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class BalancedDelimitersSelectorTests
{
    private static ISelection[] Act(string input)
        => Select.Balanced('(', ')').Delimiters().Extract(input);

    private IndexSet GetIndexSetAt(ISelection[] selections, int index)
    {
        var selection = selections[index];
        var indexSet = Assert.IsType<IndexSet>(selection);
        return indexSet;
    }

    [Fact]
    public void Simple()
    {
        var input = "(1)";
        var selections = Act(input);
        Assert.Single(selections);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([0, 2], indexSet.Indices);
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selections = Act(input);
        Assert.Single(selections);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([4, 11], indexSet.Indices);
    }

    [Fact]
    public void Siblings()
    {
        var input = "(1)(2)";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([0, 2], indexSet.Indices);
        indexSet = GetIndexSetAt(selections, 1);
        Assert.Equal([3, 5], indexSet.Indices);
    }

    [Fact]
    public void SingleNested()
    {
        var input = "((1))";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([0, 4], indexSet.Indices);
        indexSet = GetIndexSetAt(selections, 1);
        Assert.Equal([1, 3], indexSet.Indices);
    }

    [Fact]
    public void NestedAgain()
    {
        var input = "(1(2)3)";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([0, 6], indexSet.Indices);
        indexSet = GetIndexSetAt(selections, 1);
        Assert.Equal([2, 4], indexSet.Indices);
    }

    [Fact]
    public void Complex()
    {
        var input = "1 + (2 + (3 * 4))";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var indexSet = GetIndexSetAt(selections, 0);
        Assert.Equal([4, 16], indexSet.Indices);
        indexSet = GetIndexSetAt(selections, 1);
        Assert.Equal([9, 15], indexSet.Indices);
    }
}