using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class BalancedInsideSelectorTests
{
    private static ISelection[] Act(string input)
        => Select.Balanced('(', ')').Inside().Extract(input);

    private static Slice GetSliceAt(ISelection[] selections, int index)
    {
        var selection = selections[index];
        var slice = Assert.IsType<Slice>(selection);
        return slice;
    }

    [Fact]
    public void Simple()
    {
        var input = "(1)";
        var selections = Act(input);
        Assert.Single(selections);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(1, slice.Start);
        Assert.Equal(1, slice.Length);
    }

    [Fact]
    public void OneTwoThree()
    {
        var input = "1 + ( 2 + 3)";
        var selections = Act(input);
        Assert.Single(selections);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(5, slice.Start);
        Assert.Equal(6, slice.Length);
    }

    [Fact]
    public void Siblings()
    {
        var input = "(1)(2)";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(1, slice.Start);
        Assert.Equal(1, slice.Length);
        slice = GetSliceAt(selections, 1);
        Assert.Equal(4, slice.Start);
        Assert.Equal(1, slice.Length);
    }

    [Fact]
    public void SingleNested()
    {
        var input = "((1))";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(1, slice.Start);
        Assert.Equal(3, slice.Length);
        slice = GetSliceAt(selections, 1);
        Assert.Equal(2, slice.Start);
        Assert.Equal(1, slice.Length);
    }

    [Fact]
    public void NestedAgain()
    {
        var input = "(1(2)3)";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(1, slice.Start);
        Assert.Equal(5, slice.Length);
        slice = GetSliceAt(selections, 1);
        Assert.Equal(3, slice.Start);
        Assert.Equal(1, slice.Length);
    }

    [Fact]
    public void Complex()
    {
        var input = "1 + (2 + (3 * 4))";
        var selections = Act(input);
        Assert.Equal(2, selections.Length);
        var slice = GetSliceAt(selections, 0);
        Assert.Equal(5, slice.Start);
        Assert.Equal(11, slice.Length);
        slice = GetSliceAt(selections, 1);
        Assert.Equal(10, slice.Start);
        Assert.Equal(5, slice.Length);
    }
}