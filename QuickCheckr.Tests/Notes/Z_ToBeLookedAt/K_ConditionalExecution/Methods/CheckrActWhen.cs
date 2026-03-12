using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_ConditionalExecution.Methods;

[DocFile]
public class CheckrActWhen : BaseTest
{
    [Fact]
    [DocExample(typeof(CheckrActWhen), nameof(GetCheckr))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(625676917, 3.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<CheckrActWhen>(article, true);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
            from never in Checkr.ActWhen("Never", () => false, () => { })
            from neverAgain in Checkr.ActWhen("Never Again", () => false, () => true)
            from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
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

    [Fact]
    [DocExample(typeof(CheckrActWhen), nameof(GetCheckrTask))]
    public void RunTestTask()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrTask().Run(625676917, 3.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<CheckrWhen>(article, false);
        VerifyTask(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrTask()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
            from never in Checkr.ActWhen("Never", () => false, () => Task.CompletedTask)
            from neverAgain in Checkr.ActWhen("Never Again", () => false, () => Task.FromResult(true))
            from thing in Checkr.ActWhen("Act", () => counter != 2, () => Task.FromResult(counter == 3 ? null : new Thing()))
            from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
            select Case.Closed;
    }

    private static void VerifyTask(Article article)
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