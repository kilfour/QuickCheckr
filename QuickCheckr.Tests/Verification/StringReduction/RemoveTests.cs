using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class RemoveTests
{
    [Fact]
    public void RemovesASelectedCharacter()
    {
        var selector = Stringr.Once('a').Remove();
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Equal("a", result.Value.Text);
        Assert.Equal("bc", state.Text);
        Assert.Equal(0, state.Index);
        Assert.True(state.Changed);
    }

    [Fact]
    public void RemovesASelectedRun()
    {
        var selector = Stringr.AtleastOnce('a').Remove().Defined();

        var result = StringReductionTestTools.RunAll(selector, "baaac");

        Assert.Equal("bc", result);
    }

    [Fact]
    public void MismatchLeavesTheTextUnchanged()
    {
        var selector = Stringr.Once('a').Remove().Defined();

        var result = StringReductionTestTools.RunAll(selector, "bbb");

        Assert.Equal("bbb", result);
    }

    [Fact]
    public void ChainedRemovalsAreAppliedAsOneCandidate()
    {
        var selector =
            from first in Stringr.Once('a').Remove()
            from second in Stringr.Once('b').Remove()
            select Selection.Defined;

        var candidates = StringReductionTestTools.RunCandidates(selector, "abc");

        Assert.Equal(["c", "c"], candidates);
    }
}
