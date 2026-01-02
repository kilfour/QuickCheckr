using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_Reducers.B_Full;

[DocFile]
public class MultiStepReducers : CheckrTest<MultiStepReducers>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(2042459799);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from text in Checkr.Input("Text", Fuzzr.String(), new StringShortener().Reducer)
        from trace in Checkr.Trace("Original", () => text)
        from expect in Checkr.Expect("no ab", () => !text.Contains("ab"))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("no ab", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Text", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"nhrmdkzabi\"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("\"ab\"", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("nhrmdkzabi", article.Execution(1).Trace(1).Read().Value);
    }

    public class StringShortener
    {
        private enum State { Left, Right }
        private State Step = State.Left;
        public Reduction Reducer(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (str.Length == 1)
                return Reduction.Stop();
            if (result.Failed)
            {
                if (Step == State.Left)
                    return Reduction.Next(str[1..]);
                else
                    return Reduction.Next(str[..^1]);
            }
            if (Step == State.Left)
            {
                Step = State.Right;
                return Reduction.Next(str[..^1]);
            }

            return Reduction.Stop();
        }
    }
}
