using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"This is the second step in QuickCheckr shrinking, but it needs to be explicitely activated
by passing in a configuration function to the Run method: `a => a with { ShrinkingActions = true }`.  

It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  

*Note:* This is still a very experimental feature, hence the opt-in and leads to problems in some cases.  
Investigation is ongoing ...

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
            GetCheckr().Run(a => a with { ShrinkMode = ShrinkMode.ExceptReduction }));
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
    [DocHeader("Elaborating")]
    [DocContent(
@"In very rare occasions *Action Shrinking* can be too greedy.  
Consider the following example:")]
    [DocExample(typeof(ActionShrinking), nameof(Elaborating_Checkr))]
    [DocContent(
@"QuickCheckr replays the run and deactivates the action to ascertain its relevance.  
However, the action returns a value which other `LINQ` operations might depend on, so QuickCheckr returns a `default` value.  

But, and here's the confusing bit, the spec in our example fails if the action returns false, the default bool value,
so the spec fails even if the action is deactivated,
and hence QuickCheckr decides it is irrelevant and happily removes it from the run.  

And here we find ourselves confused ;-).  

!!! No LONGER THE CASE SEE THE REPORT: !!!
")]
    [DocReportHeader]
    [DocCodeFile("Elaborating.txt", "text")]
    public void Elaborating()
    {
        var article = TheJournalist.Exposes(() =>
            Elaborating_Checkr().Run(a => a with { ShrinkMode = ShrinkMode.All }));
        WriteAsserts(article);
        WriteReport(article, "Elaborating.txt", false);
        Assert.Equal("Act returns true", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions()); // <=
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Elaborating_Checkr()
    {
        return
            from act in Checkr.ShrinkableAct("Act", () => false)
            from spec in Checkr.Expect("Act returns true", () => act.Result == true)
            select Case.Closed;
    }

    [Fact]
    [DocHeader("Workaround")]
    [DocContent(
@"No Longer Needed !!!")]
    [DocExample(typeof(ActionShrinking), nameof(ElaboratingFix_Checkr))]
    public void ElaboratingFix()
    {
        var article = TheJournalist.Exposes(() =>
            ElaboratingFix_Checkr().Run(a => a with { ShrinkMode = ShrinkMode.All }));
        WriteAsserts(article);
        Assert.Equal("Act returns true", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> ElaboratingFix_Checkr()
    {
        return
            from act in Checkr.ShrinkableAct("Act", () => false)
            from spec in Checkr.Expect("Act returns true", () => act.Result == true)
            select Case.Closed;
    }
}
