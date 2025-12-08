using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking.A_ExecutionShrinking;

[DocFile]
[DocContent("This is the first step in QuickCheckr shrinking.")]
public class ExecutionShrinking : BaseTest
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

So after *Execution Shrinking*, the result is:
")]
    [DocCodeFile("ExecutionShrinking.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(ExampleTest);
        WriteAsserts(article);
        WriteReport<ExecutionShrinking>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedSpec(1).Read().Label);
        Assert.Equal(2, article.PassedSpec(1).Read().TimesPassed);
    }

    private static void ExampleTest()
    {
        Example_Check().Run(
            1.Runs(),
            50.ExecutionsPerRun(),
            a => a with { Seed = 669391953 });
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
            from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
            from spec in Checkr.Spec("Counter does not equal 3", () => counter != 3)
            select Case.Closed;
    }
}
