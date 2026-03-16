using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
This uses a custom reducer to shrink the input.  
It currently uses the full reducer algorithm.  
Working on turning this into a much easier way to define the reducer.  
But chaining reducers is still a work in progress.  
")]
public class Test2Better : CheckrTest<Test2Better>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(PairReducer))]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(737468544);

    public record Pair(int A, int B);

    private static readonly FuzzrOf<Pair> TheFuzzr =
        from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
        from pair in Fuzzr.One<Pair>()
        select pair;

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", TheFuzzr,
            Shrink.OnType<Pair>(s => s.Simplify(PairReducer.TowardsZero)))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "Difference must not be small.",
            () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
        select Case.Closed;

    [CodeExample]
    private class PairReducer
    {
        public static Func<Reducer<Pair>> TowardsZero = () => new PairReducer().Reduce;
        private enum Phase { Both, A, B }
        private Phase phase = Phase.Both;
        public Reduction Reduce(ReductionResult result)
        {
            var current = result.ValueAs<Pair>();
            if (result.Passed)
            {
                if (phase == Phase.Both)
                    phase = Phase.A;
                else if (phase == Phase.A)
                    phase = Phase.B;
                if (phase == Phase.B)
                    return Reduction.Stop(current);
            }
            switch (phase)
            {
                case Phase.Both:
                    if (current.A <= 0 || current.B == 0)
                        return Reduction.Stop(current);
                    return Reduction.Next(current with { A = current.A - 1, B = current.B - 1 });
                case Phase.A:
                    if (current.A <= 0)
                        return Reduction.Stop(current);
                    return Reduction.Next(current with { B = current.B - 1 });
                case Phase.B:
                    if (current.B <= 0)
                        return Reduction.Stop(current);
                    return Reduction.Next(current with { A = current.A - 1 });
            }
            return Reduction.Stop(current);
        }
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference must not be small.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 53, B: 51 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 6 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: 53, B: 51 }", article.Execution(1).Trace(1).Read().Value);
    }
}