using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;

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
    [DocCodeFile("ActionShrinking.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(ExampleTest);

        Assert.Equal("BOOM", article.Exception().Message);
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);
        var action = article.Execution(1).Action(1).Read();
        Assert.Equal("Act Three", action.Label);

        var report = TheClerk.Transcribes(article.CaseFile);
        var reader = LinesReader.FromText(report);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Test:                    ExampleTest", reader.NextLine());
        Assert.Contains(" Location: ", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Minimal failing case:    1 execution (after 2 shrinks)", reader.NextLine());
        Assert.Equal(" Seed:                    699025678", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (1): Act Three", reader.NextLine());
        Assert.Equal(" ===========================================================================", reader.NextLine());
        Assert.Equal("  !! Exception Thrown: System.Exception: BOOM", reader.NextLine());
        // Skip the stack trace. 
        reader.Skip(3);
        // --
        Assert.Equal(" ===========================================================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
        WriteReport.ToFile("ActionShrinking.txt", report);
    }

    private static void ExampleTest()
    {
        Example_Check().Run(
            1.Runs(),
            50.ExecutionsPerRun(),
            a => a with { Seed = 699025678 });
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
