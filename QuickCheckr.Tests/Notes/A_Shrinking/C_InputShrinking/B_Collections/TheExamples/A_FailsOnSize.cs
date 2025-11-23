using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class A_FailsOnSize : BaseTest
{
    [Fact]
    [DocExample(typeof(A_FailsOnSize), nameof(GetCheckr))]
    [DocCodeFile("A_FailsOnSize.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1833095082 }));
        WriteAsserts(article);
        WriteReport<A_FailsOnSize>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List does not contain 7", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 7 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
            from spec in Checkr.Spec("List does not contain 7", () => !listInput.Contains(7))
            from trace in Checkr.Trace("List Input", listInput)
            select Case.Closed;
    }
}