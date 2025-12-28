using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"Ofcourse a collection itself is just an input like another, so if it, as a whole,
is irrelevant for the failure, QuickCheckr will remove it.")]
[DocExample(typeof(B_RemovingIrrelevantCollection), nameof(GetCheckr))]
[DocCodeFile("B_RemovingIrrelevantCollection.txt", "text")]
public class B_RemovingIrrelevantCollection : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(2014608033));
        WriteAsserts(article);
        WriteReport<B_RemovingIrrelevantCollection>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Always fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
            from expect in Checkr.Expect("Always fails", () => false)
            select Case.Closed;
    }
}