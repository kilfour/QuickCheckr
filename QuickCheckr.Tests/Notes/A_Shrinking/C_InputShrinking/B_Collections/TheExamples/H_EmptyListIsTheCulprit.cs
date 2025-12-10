using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class H_EmptyListIsTheCulprit : BaseTest
{
    [Fact]
    [DocExample(typeof(H_EmptyListIsTheCulprit), nameof(GetCheckr))]
    [DocCodeFile("H_EmptyListIsTheCulprit.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(2077876521, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<H_EmptyListIsTheCulprit>(article, false);
        ExplainMe<H_EmptyListIsTheCulprit>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
            from listNotEmpty in Checkr.Spec("List is not empty", () => list.Any())
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List is not empty", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[  ]", article.Execution(1).Input(1).Read().Value);
    }
}