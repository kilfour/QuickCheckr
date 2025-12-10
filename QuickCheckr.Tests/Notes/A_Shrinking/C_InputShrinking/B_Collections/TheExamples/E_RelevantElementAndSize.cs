using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class E_RelevantElementAndSize : BaseTest
{
    [Fact]
    [DocExample(typeof(E_RelevantElementAndSize), nameof(GetCheckr))]
    [DocCodeFile("E_RelevantElementAndSize.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1058309953));
        WriteAsserts(article);
        WriteReport<E_RelevantElementAndSize>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Second element is not 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 42 ]", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>([666, 42]))
            from spec in Checkr.SpecIf("Second element is not 42", () => input.Count() > 1, () => input.ElementAt(1) != 42)
            select Case.Closed;
    }
}