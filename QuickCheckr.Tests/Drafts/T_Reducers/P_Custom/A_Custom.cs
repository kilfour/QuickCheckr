using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_Reducers.P_Custom;

[DocFile]
public class A_Custom : CheckrTest<A_Custom>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("Custom Reducer")]
    [DocExample(typeof(CustomTowardsZeroReducer))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(211010750);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(), new CustomTowardsZeroReducer().Reduce)
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
    public class CustomTowardsZeroReducer
    {
        private int lastFailed = 0;
        public Reduction Reduce(ReductionResult result)
        {
            var value = result.ValueAs<int>();
            if (result.Failed)
                lastFailed = value;
            else
                return Reduction.Stop(lastFailed);
            if (value == 0)
                return Reduction.Stop(lastFailed);
            if (value > 0)
                value--;
            else
                value++;
            return Reduction.Next(value);
        }
    }
}
