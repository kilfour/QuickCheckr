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
    public async Task CompoundReplacementStopsAtTheLastFailingCandidate()
    {
        var selector =
            from selected in Stringr.Once('a', 'b')
            from replaced in Stringr.Once('c', 'd').Replace(_ => selected.Text.ToUpperInvariant())
            select Selection.Defined;

        var reduction = await StringReductionTestTools.ReduceAsync(
            selector,
            "--ac--bd--",
            candidate => ((string)candidate).Contains('d'));

        var stop = Assert.IsType<Reduction.Stop>(reduction);
        Assert.Equal("--aA--bd--", stop.Result);
    }

    [Fact]
    public async Task RemovesLeftHandSidesWhileTheFailureMarkerRemains()
    {
        var reduction = await StringReductionTestTools.ReduceAsync(
            RemoveLeftHandSideAndOperator,
            "3+2-(3+1#-2)",
            candidate => ((string)candidate).Contains('#'));

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("(1#-2)", unbounded.Result);
    }

    [Fact]
    public async Task RemovesLeftHandSidesWhenEveryCandidateFails()
    {
        var reduction = await StringReductionTestTools.ReduceAsync(
            RemoveLeftHandSideAndOperator,
            "3+2-(3+1#-2)",
            _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(reduction);
        Assert.Equal("(1#-2)", unbounded.Result);
    }
}
