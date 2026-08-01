using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class StateTests
{
    [Fact]
    public void AdvanceAndRestartControlTheCursor()
    {
        var state = new SelectrState("abc");

        state.Advance(2);
        state.Changed = true;
        state.Restart(1);

        Assert.Equal(1, state.Index);
        Assert.False(state.Changed);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public void BoundsAreRelativeToTheCursor(int offset, bool expected)
    {
        var state = new SelectrState("abc");

        Assert.Equal(expected, state.InBounds(offset));
        Assert.Equal(!expected, state.OutOfBounds(offset));
    }

}
