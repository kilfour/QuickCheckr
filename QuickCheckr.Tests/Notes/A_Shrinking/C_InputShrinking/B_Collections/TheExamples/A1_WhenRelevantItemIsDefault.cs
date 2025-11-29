using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class A1_WhenRelevantItemIsDefault : BaseTest
{
    [Fact]
    [DocExample(typeof(A1_WhenRelevantItemIsDefault), nameof(GetCheckr))]
    [DocCodeFile("A1_WhenRelevantItemIsDefault.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 820833320 }));
        WriteAsserts(article);
        WriteReport<A1_WhenRelevantItemIsDefault>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List does not contain 0", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 0 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 84, 63, 90, 91, 55, 53, 39, 91, 0 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
            from spec in Checkr.Spec("List does not contain 0", () => !listInput.Contains(0))
            from trace in Checkr.Trace("List Input", () => listInput)
            select Case.Closed;
    }


    [Fact]
    [DocExample(typeof(A1_WhenRelevantItemIsDefault), nameof(GetCheckr_Negated))]
    [DocCodeFile("A1_WhenRelevantItemIsDefault_Negated.txt", "text")]
    public void RunTest_Negated()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr_Negated().Run(a => a with { Seed = 816312044 }));
        WriteAsserts(article);
        WriteReport(article, "A1_WhenRelevantItemIsDefault_Negated.txt");
        Verify_Negated(article);
    }

    private static void Verify_Negated(Article article)
    {
        Assert.Equal("List contains only 0", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 51 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 28, 0, 50, 27, 37, 54, 74, 56, 51 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Negated()
    {
        return
            from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
            from spec in Checkr.Spec("List contains only 0", () => listInput.All(a => a == 0))
            from trace in Checkr.Trace("List Input", () => listInput)
            select Case.Closed;
    }
}