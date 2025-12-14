using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class T_ArrayShrinking : BaseTest
{
    [Fact(Skip = "WIP")]
    [DocExample(typeof(T_ArrayShrinking), nameof(GetCheckr))]
    [DocCodeFile("T_ArrayShrinking.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1100967994));
        WriteAsserts(article);
        WriteReport<T_ArrayShrinking>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("int[0] is not 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("int[]", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 42 ]", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> GetCheckr()
    {
        var array = new[] { 42, 2, 3 };
        var check =
            from input in Checkr.Input("int[]", Fuzzr.Constant(array))
            from expect in Checkr.ExpectWhen("int[0] is not 42",
                () => input.Length != 0,
                () => input.First() != 42)
            select Case.Closed;
        return check;
    }
}