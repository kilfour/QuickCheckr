using QuickCheckr.StringReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class ArithmeticReducerTests
{
    private static readonly SelectrOf<ITextSelection> Operator =
        Stringr.Once('+', '-', '*', '/', '^');

    private static readonly SelectrOf<ITextSelection> Number =
        Stringr.AtleastOnce(char.IsDigit);

    private static readonly SelectrOf<Selection> RemoveLeftHandSideAndOperator =
        from leftHandSide in Number.Remove()
        from op in Operator.Remove()
        select Selection.Defined;

    [Fact]
    public void CompoundReplacementStopsAtTheLastFailingCandidate()
    {
        var selector =
            from selected in Stringr.Once('a', 'b')
            from replaced in Stringr.Once('c', 'd').Replace(_ => selected.Text.ToUpperInvariant())
            select Selection.Defined;

        var reduction = StringReductionTestTools.Reduce(
            selector,
            "--ac--bd--",
            candidate => ((string)candidate).Contains('d'));

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal("--aA--bd--", stop.Result);
    }

    [Fact]
    public void RemovesLeftHandSidesWhileTheFailureMarkerRemains()
    {
        var reduction = StringReductionTestTools.Reduce(
            RemoveLeftHandSideAndOperator,
            "3+2-(3+1#-2)",
            candidate => ((string)candidate).Contains('#'));

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("(1#-2)", unbounded.Result);
    }

    [Fact]
    public void RemovesLeftHandSidesWhenEveryCandidateFails()
    {
        var reduction = StringReductionTestTools.Reduce(
            RemoveLeftHandSideAndOperator,
            "3+2-(3+1#-2)",
            _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("(1#-2)", unbounded.Result);
    }
}
