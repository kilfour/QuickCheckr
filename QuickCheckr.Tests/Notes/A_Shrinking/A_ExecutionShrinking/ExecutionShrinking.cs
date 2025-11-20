using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.A_ExecutionShrinking;

[DocFile]
[DocContent("This is the first step in QuickCheckr shrinking.")]
public class ExecutionShrinking
{
    [Fact]
    [DocContent("The following simple example fails on the third execution.")]
    [DocExample(typeof(ExecutionShrinking), nameof(Example_Check))]
    [DocContent(
@"
Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after Execution Shrinking, the result is:
")]
    [DocCode(
@"
 ------------------------------------------------------------
 Test:                    Example
 Location:                ExecutionShrinking.cs:28:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1464227412
 ------------------------------------------------------------
  Executed (3):
   - Input: counter = 3
 ==================================
  !! Spec Failed: Fails on Three
 ==================================", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() => Example_Check().Run(1.Runs(), 50.ExecutionsPerRun()));
        Assert.Equal("Fails on Three", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        var execution = article.Execution(1).Read();
        Assert.Equal(3, execution.ExecutionId);
        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("counter", input.Label);
        Assert.Equal("3", input.Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
            from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
            from spec in Checkr.Spec("Fails on Three", () => counter != 3)
            select Case.Closed;
    }
}