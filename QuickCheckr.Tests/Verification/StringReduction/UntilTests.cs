using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class UntilTests
{
    [Fact]
    public void ConsumesTextBeforeTheDelimiter()
    {
        var state = new SelectrState("abc)def");

        var result = Stringr.Until(')')(state);

        Assert.Equal("abc", result.Value.Text);
        Assert.Equal(3, state.Index);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ConsumesTheRemainderWhenThereIsNoDelimiter()
    {
        var state = new SelectrState("abc");

        var result = Stringr.Until(')')(state);

        Assert.Equal("abc", result.Value.Text);
        Assert.Equal(3, state.Index);
    }

    [Fact]
    public void AllowsAnEmptyMatchAtTheDelimiter()
    {
        var state = new SelectrState(")abc");

        var result = Stringr.Until(')')(state);

        Assert.Equal(string.Empty, result.Value.Text);
        Assert.Equal(0, state.Index);
        Assert.True(result.Succeeded);
    }
}
