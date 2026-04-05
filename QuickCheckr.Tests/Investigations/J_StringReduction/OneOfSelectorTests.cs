using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class OneOfSelectorTests
{
    [Fact]
    public void Null()
    {
        var input = "a";
        var selection = Select.OneOf('b').Extract(input);
        Assert.Null(selection);
    }


    [Fact]
    public void Single()
    {
        var input = "a";
        var selection = Select.OneOf('a').Extract(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("", slice.Remove(input));
    }

    [Fact]
    public void Two()
    {
        var input = "acab";
        var selection = Select.OneOf('a').OneOf('b').Extract(input);
        var slice = Assert.IsType<Slice>(selection);
        Assert.Equal("ac", slice.Remove(input));
    }
}