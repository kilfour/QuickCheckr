using QuickCheckr;
using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class JustChecking
{
    private static string RunAll(SelectrOf<Selection> selector, string input, int maxSteps = 32)
    {
        var result = input;
        var runner = selector.Run(input);
        var steps = 0;
        while (!runner.EndOfText && steps < maxSteps)
        {
            result = runner.Next();
            steps++;
        }
        return result;
    }

    [Fact]
    public void Or_RestoresText_WhenLeftBranchMutatesThenFails()
    {
        var left =
            from removed in Stringr.Once('a').Remove()
            from x in Stringr.Once('x')
            select removed;

        var right = Stringr.Once('a');

        var selectr = left.Or(right).Remove().Defined();

        Assert.Equal("b", RunAll(selectr, "ab"));
    }

    [Fact]
    public void Or_RestoresOffset_WhenLeftBranchReplacesThenFails()
    {
        var left =
            from replaced in Stringr.Once('a').Replace(_ => "zzz")
            from x in Stringr.Once('x')
            select replaced;

        var selectr =
            from chosen in left.Or(Stringr.Once('a')).Editable()
            from replacement in chosen.Replace(_ => "A")
            select Selection.Defined;

        Assert.Equal("Ab", RunAll(selectr, "ab"));
    }

    [Fact]
    public void Balanced_UnterminatedInput_DoesNotThrow_AndLeavesTextUnchanged()
    {
        var selectr = Stringr.Balanced('(', ')').Remove().Defined();
        var runner = selectr.Run("(ab");

        var result = Record.Exception(() => runner.Next());

        Assert.Null(result);
        Assert.Equal("(ab", RunAll(selectr, "(ab"));
    }

    [Fact]
    public void Once_NoMatch_LeavesTextUnchanged()
    {
        var selectr = Stringr.Once('a').Remove().Defined();

        Assert.Equal("bbb", RunAll(selectr, "bbb"));
    }

    [Fact]
    public void AtleastOnce_NoMatch_LeavesTextUnchanged()
    {
        var selectr = Stringr.AtleastOnce('a').Remove().Defined();

        Assert.Equal("bbb", RunAll(selectr, "bbb"));
    }

    [Fact]
    public void OptionalDefined_NoMatch_ShouldReachEndOfText()
    {
        var state = new SelectrState("bbb");
        var result = Stringr.Optional('a').Defined()(state);

        Assert.Equal(Selection.Defined, result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(state.RunStopped);
    }

    [Fact]
    public void UntilDefined_DelimiterAtCurrentPosition_ShouldReachEndOfText()
    {
        var state = new SelectrState(")x");
        var result = Stringr.Until(')').Defined()(state);

        Assert.Equal(Selection.Defined, result.Value);
        Assert.Equal(0, state.Index);
        Assert.False(state.RunStopped);
    }
}
