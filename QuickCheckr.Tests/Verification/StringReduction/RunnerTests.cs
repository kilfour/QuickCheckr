using QuickCheckr.StringReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class RunnerTests
{
    [Fact]
    public void SearchesPastAnUnmatchedPrefix()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();
        var run = selector.Run("cba");

        var candidate = run.Next();

        Assert.Equal("ca", candidate);
    }

    [Fact]
    public void SuccessiveCandidatesBuildOnAcceptedChanges()
    {
        var selector = Stringr.Once('a', 'b').Remove().Defined();

        var candidates = StringReductionTestTools.RunCandidates(selector, "abc");

        Assert.Equal(["bc", "c", "c"], candidates);
    }

    [Fact]
    public void NoMatchReturnsTheOriginalTextAndReachesTheEnd()
    {
        var selector = Stringr.Once('a').Remove().Defined();
        var run = selector.Run("bbb");

        var candidate = run.Next();

        Assert.Equal("bbb", candidate);
        Assert.True(run.EndOfText);
    }

    [Fact]
    public void SeparateRunsHaveIndependentState()
    {
        var selector = Stringr.Once('a').Remove().Defined();
        var first = selector.Run("a1");
        var second = selector.Run("a2");

        Assert.Equal("1", first.Next());
        Assert.Equal("2", second.Next());
    }

    [Fact]
    public void EmptyOptionalMatchStillMakesProgress()
    {
        var selector = Stringr.Optional('a').Defined();
        var run = selector.Run("bbb");

        run.Next();

        Assert.True(run.EndOfText);
    }

    [Fact]
    public void UnchangedReplacementStillMakesProgress()
    {
        var selector = Stringr.Once('a').Replace(text => text).Defined();
        var run = selector.Run("ab");

        run.Next();
        run.Next();

        Assert.True(run.EndOfText);
    }

    [Fact]
    public void FailedAttemptRestoresLateReplacementOffsetsBeforeTryingTheNextPosition()
    {
        var selector =
            from first in Stringr.Once('a', 'b').Editable()
            from replacement in first.Replace(_ => "zzz")
            from suffix in Stringr.Once('c')
            select Selection.Defined;

        var candidates = StringReductionTestTools.RunCandidates(selector, "abc");

        Assert.Equal(["azzzc"], candidates);
    }
}
