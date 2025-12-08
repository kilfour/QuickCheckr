using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class D_RelevantMultipleElements : BaseTest
{
    [Fact]
    [DocExample(typeof(D_RelevantMultipleElements), nameof(GetCheckr))]
    [DocCodeFile("D_RelevantMultipleElements.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 500957855 }));
        WriteAsserts(article);
        WriteReport<D_RelevantMultipleElements>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("spec", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 1, 2, 3 ]", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("input", Fuzzr.Int(1, 10).Many(1, 10))
            from spec in Checkr.SpecIf("spec", () => input.Count() > 2, () =>
                !(input.ToList()[0] == 1
                && input.ToList()[1] == 2
                && input.ToList()[2] == 3))
            select Case.Closed;
    }
}