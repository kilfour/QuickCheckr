using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.T_CustomShrinkers;

[DocFile]
[DocExample(typeof(CustomShrinkers), nameof(GetCheckr))]
[DocCodeFile("CustomShrinkers.txt", "text")]
public class CustomShrinkers : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(2122063655));
        WriteAsserts(article);
        WriteReport<CustomShrinkers>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("One should not be 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original One", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Original Two", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("90", article.Execution(1).Trace(2).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from one in Checkr.Input("One", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
            from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
            from traceOne in Checkr.Trace("Original One", () => one)
            from traceTwo in Checkr.Trace("Original Two", () => two)
            from spec in Checkr.Expect("One should not be 42", () => one != 42)
            select Case.Closed;
    }
}