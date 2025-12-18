using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.H_OnShrinking.A_ExecutionShrinking.A_Issues;

[DocFile]
public class Issues : BaseTest
{
    [Fact]
    [DocExample(typeof(Issues), nameof(GetCheckr))]
    [DocCodeFile("Issues.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(625676917, 3.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<Issues>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
            from thing in Checkr.ActIf("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
            from notNull in Checkr.Expect("Not Null", () => thing != null)
            select Case.Closed;
    }

    public class Thing();

    private static void Verify(Article article)
    {
        Assert.Equal("Not Null", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Not Null", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [Fact]
    [DocHeader("Workaround")]
    [DocExample(typeof(Issues), nameof(GetCheckrFixed))]
    [DocCodeFile("IssuesFixed.txt", "text")]
    public void RunTestFixed()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrFixed().Run(625676917, 3.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "IssuesFixed.txt", false);
        VerifyFixed(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrFixed()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
            from thing in Checkr.ActCarefullyIf("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
            from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
            select Case.Closed;
    }

    private static void VerifyFixed(Article article)
    {
        Assert.Equal("Not Null", article.FailedExpectation());
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