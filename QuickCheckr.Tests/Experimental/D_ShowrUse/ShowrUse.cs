using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Experimental.D_ShowrUse;

[DocFile]
public class ShowrUse : BaseTest
{
    [Fact]
    [DocExample(typeof(ShowrUse), nameof(GetCheckr))]
    [DocCodeFile("ShowrUse.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(553690512, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<ShowrUse>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("int", Fuzzr.Int())
            from showOne in Showr.Use(() => Please.AllowMe().ToReplace<int>(a => $"One: {a}"))
            from traceOne in Checkr.Trace("int One", () => input)
            from showTwo in Showr.Use(() => Please.AllowMe().ToReplace<int>(a => $"Two: {a}"))
            from traceTwo in Checkr.Trace("int Two", () => input)
            from noAnswer in Checkr.Expect("no answer", () => input != 42)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("no answer", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("int", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("int One", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("One: 42", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("int Two", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("Two: 42", article.Execution(1).Trace(2).Read().Value);
    }
}