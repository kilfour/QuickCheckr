using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
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
    [DocExample(typeof(ActionShrinking), nameof(Example_Report), "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() =>
            Example_Check().Run(
                1.Runs(),
                50.ExecutionsPerRun(),
                a => a with { Seed = 699025678 }));
        Assert.Equal("BOOM", article.Exception().Message);
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);
        var action = article.Execution(1).Action(1).Read();
        Assert.Equal("Act Three", action.Label);
        // Assert.Equal(Example_Report(), TheClerk.Transcribes(article.CaseFile));
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

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    [CodeRemove("Test:                    <Example>b__0_0")]
    [CodeRemove("Location:                C:\\Code\\QuickCheckr\\QuickCheckr.Tests\\Notes\\A_Shrinking\\B_ActionShrinking\\ActionShrinking.cs:28:1")]
    private static string Example_Report() =>
@" ------------------------------------------------------------
 Test:                    <Example>b__0_0
 Location:                C:\Code\QuickCheckr\QuickCheckr.Tests\Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:28:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    699025678
 ------------------------------------------------------------
  Executed (1): Act Three
 ===========================================================================
  !! Exception Thrown: System.Exception: BOOM
   at QuickCheckr.Tests.Notes.A_Shrinking.B_ActionShrinking.ActionShrinking.<>c.<Example_Check>b__1_6() in C:\Code\QuickCheckr\QuickCheckr.Tests\Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:line 46
   at QuickCheckr.Checkr.<>c__DisplayClass1_0.<Act>b__0() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 11
   at QuickCheckr.Checkr.<>c__DisplayClass2_0`1.<TryCatch>b__0(CheckrState state) in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 27
 ===========================================================================";
}
