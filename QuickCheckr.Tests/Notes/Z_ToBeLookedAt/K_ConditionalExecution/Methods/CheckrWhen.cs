using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_ConditionalExecution.Methods;


[DocFile]
public class CheckrWhen : BaseTest
{
    [Fact]
    [DocExample(typeof(CheckrWhen), nameof(GetCheckr))]
    public void RunTest()
    {
        var article = Journalist.Investigate(GetCheckr(), a => a

            .Run(625676917, 3.ExecutionsPerRun()));

        WriteAsserts(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
            from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
            from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
            select Case.Closed;
    }

    public class Thing();

    private static void Verify(Article article)
    {
        Assert.Equal("Not Null", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Not Null", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}
