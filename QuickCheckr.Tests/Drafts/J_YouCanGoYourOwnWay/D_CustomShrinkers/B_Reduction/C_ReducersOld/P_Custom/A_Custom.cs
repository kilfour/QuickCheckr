using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.P_Custom;

[DocFile]
public class A_Custom : CheckrTest<A_Custom>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("Reducer")]
    [DocExample(typeof(CustomTowardsZero))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(211010750);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(CustomTowardsZero.Reducer())))
        from expect in Checkr.Expect("Input < 2", () => input < 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input < 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("10", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Redux.Value);
    }

    [CodeExample]
    public class CustomTowardsZero
    {
        public static Func<Reducer<int>> Reducer() =>
            () => new CustomTowardsZero().Reduce;

        private int lastFailed = 0;

        public Reduction Reduce(ReductionResult result)
        {
            var value = result.ValueAs<int>();
            if (result.Failed)
                lastFailed = value;
            else
                return new Reduction.Stop(lastFailed);
            if (value == 0)
                return new Reduction.Stop(lastFailed);
            if (value > 0)
                value--;
            else
                value++;
            return new Reduction.Next(value);
        }
    }
}
