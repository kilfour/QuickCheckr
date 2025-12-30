using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.A_Primitives.A_NullString;

[DocFile]
public class NullString : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.String())
            from expect in Checkr.Expect("Fails", () => input.Length > 0 && false)
            select Case.Closed;
    }

    [Fact]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(NullString), nameof(GetCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("NullString.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(393010076, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<NullString>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }
}