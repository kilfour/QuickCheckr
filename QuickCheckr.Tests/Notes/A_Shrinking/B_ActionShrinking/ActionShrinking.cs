using QuickCheckr.Tests.Notes.A_Shrinking.A_ExecutionShrinking;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"This is the second step in QuickCheckr shrinking.  
It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  
In the same vein as with Execution Shrinking, Actions are also reevaluated and judged to be relevant for the failure.")]
public class ActionShrinking
{
    [Fact]
    [DocContent("The following simple example fails on the third action.")]
    [DocExample(typeof(ActionShrinking), nameof(Example_Check))]
    [DocContent(
@"
Execution 1 - Action 1: noop  
Execution 1 - Action 2: noop  
Execution 1 - Action 3: Throws => (Fails)  

So after Action Shrinking, the result is:
")]
    [DocCode(
@"
------------------------------------------------------------
 Test:                    Example
 Location:                ActionShrinking.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1272794573
 ------------------------------------------------------------
  Executed (1): Act Three
 ===========================================================================
  !! Exception Thrown: System.Exception: BOOM
 ===========================================================================
", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() => Example_Check().Run(1.Runs(), 50.ExecutionsPerRun()));
        Assert.Equal("BOOM", article.Exception().Message);
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);
        var action = article.Execution(1).Action(1).Read();
        Assert.Equal("Act Three", action.Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
            from actOne in Checkr.Act("Act One", () => { })
            from actTwo in Checkr.Act("Act Two", () => { })
            from actThree in Checkr.Act("Act Three", () => throw new Exception("BOOM"))
            select Case.Closed;
    }
}