using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"This is the second step in QuickCheckr shrinking, but it needs to be explicitely activated
by passing in a configuration with ShrinkMode including ShrinkMode.Actions.  

It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  

*Note:* This is still a very experimental feature, hence the opt-in.

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
        var article = Journalist.Publish(GetCheckr(), a => a
            .Configure(a => a with { ShrinkMode = ShrinkMode.ExceptReduction })
            .Run());

        WriteAsserts(article);
        WriteReport<ActionShrinking>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act Three", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from actOne in Checkr.ShrinkableAct("Act One", () => { })
            from actTwo in Checkr.ShrinkableAct("Act Two", () => { })
            from actThree in Checkr.ShrinkableAct("Act Three", () => throw new Exception("BOOM"))
            select Case.Closed;
    }

    [Fact]
    public void Elaborating() // Shrinkable act deals with default values correctly
    {
        var article = Journalist.Publish(Elaborating_Checkr(), a => a
            .Configure(a => a with { ShrinkMode = ShrinkMode.All })
            .Run());

        WriteAsserts(article);
        WriteReport(article, "Elaborating.txt", false);
        Assert.Equal("Act returns true", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions()); // <=
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    private static CheckrOf<Case> Elaborating_Checkr()
    {
        return
            from act in Checkr.ShrinkableAct("Act", () => false)
            from spec in Checkr.Expect("Act returns true", () => act.Result == true)
            select Case.Closed;
    }
}
