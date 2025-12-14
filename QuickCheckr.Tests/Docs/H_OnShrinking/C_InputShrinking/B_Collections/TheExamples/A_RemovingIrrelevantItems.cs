using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one, replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a list contribute to the failing condition, QuickCheckr will aggressively trim the rest away. The result is a minimal list containing only the values that still trigger the failure, even if the original list was large or noisy.

Consider a test that forbids the value `7`:")]
[DocExample(typeof(A_RemovingIrrelevantItems), nameof(GetCheckr))]
[DocContent(@"Even if the original list was long and varied, QuickCheckr isolates the single relevant element:")]
[DocCodeFile("A_RemovingIrrelevantItems.txt", "text")]
public class A_RemovingIrrelevantItems : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1833095082));
        WriteAsserts(article);
        WriteReport<A_RemovingIrrelevantItems>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List does not contain 7", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
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
            from spec in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
            from trace in Checkr.Trace("List Input", () => listInput)
            select Case.Closed;
    }
}