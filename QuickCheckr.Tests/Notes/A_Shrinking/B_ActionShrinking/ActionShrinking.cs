using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"This is the second step in QuickCheckr shrinking, but it needs to be explicitely activated
by passing in a configuration function to the Run method: `a => a with { ShrinkingActions = true }`.  

It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  

In the same vein as with *Execution Shrinking*, Actions are also evaluated and judged to be relevant for the failure.")]
public class ActionShrinking : BaseTest
{
    [Fact]
    [DocContent("The following simple example fails on the third action.")]
    [DocExample(typeof(ActionShrinking), nameof(GetCheckr))]
    [DocContent(
@"
Execution 1 - Action 1: noop  
Execution 1 - Action 2: noop  
Execution 1 - Action 3: Throws => (Fails)  

So after *Action Shrinking*, the result is:
")]
    [DocCodeFile("ActionShrinking.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 50.ExecutionsPerRun(),
                a => a with
                {
                    Seed = 699025678,
                    ShrinkingActions = true
                }));
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act Three", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from actOne in Checkr.Act("Act One", () => { })
            from actTwo in Checkr.Act("Act Two", () => { })
            from actThree in Checkr.Act("Act Three", () => throw new Exception("BOOM"))
            select Case.Closed;
    }

    [Fact]
    [DocHeader("Elaborating")]
    [DocContent(
@"The reason *Action Shrinking* is not activated by default is because in some rare cases it can lead to confusion.  
Consider the following example:")]
    [DocExample(typeof(ActionShrinking), nameof(Elaborating_Checkr))]
    [DocContent(
@"If we enable *Action Shrinking* QuickCheckr replays the run and deactivates the action to ascertain its relevance.  
However, the action returns a value which other `LINQ` operations might depend on, so QuickCheckr returns a `default` value.  

But, and here's the confusing bit, the spec in our example fails if the action returns false, the default bool value,
so the spec fails even if the action is deactivated, and hence QuickCheckr decides it is irrelevant and happily removes it from the run.  

And here we find ourselves confused ;-).  
")]
    public void Elaborating()
    {
        var article = TheJournalist.Exposes(() =>
            Elaborating_Checkr().Run(a => a with { ShrinkingActions = true }));
        Assert.Equal("Act returns true", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Elaborating_Checkr()
    {
        return
            from act in Checkr.Act("Act", () => false)
            from spec in Checkr.Spec("Act returns true", () => act == true)
            select Case.Closed;
    }
}
