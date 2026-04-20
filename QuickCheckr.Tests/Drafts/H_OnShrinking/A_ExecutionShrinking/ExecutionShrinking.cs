using QuickCheckr.Tests.Drafts.H_OnShrinking.C_InputShrinking;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.A_ExecutionShrinking;

[DocFile]
[DocLink(typeof(OnShrinking))]
[DocLink(typeof(InputShrinking))]
[DocContent("[On Shrinking][OnShrinking] | [Next][InputShrinking]  \n")]
[DocContent("This is the first step in QuickCheckr shrinking.")]
public class ExecutionShrinking : CheckrTest<ExecutionShrinking>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("The following simple example fails on the third execution.")]
    [DocCheckr]
    [DocContent(
@"
Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after *Execution Shrinking*, the result is:
")]
    [DocReport]
    public override void Example()
        => Run(669391953, 3.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}
