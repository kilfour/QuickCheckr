using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.Q_Tracing;


[DocFile]
public class FormerInTheGuide : BaseTest
{
    [Fact]
    [DocHeader("Not Without a Trace")]
    [DocContent(
@"QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparency and discoverability as a first class concern:")]
    [DocExample(typeof(FormerInTheGuide), nameof(NotWithoutATrace_Checkr))]
    [DocContent("By adding a `Checkr.Trace` to the `LINQ` chain, we get the following report:")]
    [DocCodeFile("NotWithoutATrace.txt", "text")]
    public void NotWithoutATrace()
    {
        var article = Journalist.Publish(NotWithoutATrace_Checkr(), a => a.Run(769641533));
        WriteAsserts(article);
        WriteReport(article, "NotWithoutATrace.txt");
        Assert.Equal("Input /= 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("47", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> NotWithoutATrace_Checkr()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Int())
            from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from expect in Checkr.Expect("Input /= 42", () => input != 42)
            from trace in Checkr.Trace("Irrelevant Input", () => irrelevant)
            select Case.Closed;
        return check;
    }
}