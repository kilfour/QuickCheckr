using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.T_Cookbook.A_ListReverse;

[DocFile]
[DocFileHeader("Verifying List Reversal (Double-Reverse Law)")]
[DocContent(
@"**Demonstrates:**
- Custom Fuzzrs
- Relevant input shrinking
- Double-application property


I guess this one is very much tradition in PBT world, and who am I to argue with tradition, ... so here we go.

**The (buggy) implementation:**")]
[DocExample(typeof(TheReverser))]
public class ListReverse : BaseTest
{
    [CodeExample]
    public class TheReverser
    {
        public static List<int> Reverses(List<int> list)
            => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];

        private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
            => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
    }

    [Fact]
    [DocContent("**The Fuzzr:**")]
    [DocExample(typeof(ListReverse), nameof(ListFuzzr))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(ListReverse), nameof(GetCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("ListReverse.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(498116108, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<ListReverse>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return listFuzzr;")]
    private static FuzzrOf<List<int>> ListFuzzr()
    {
        var listFuzzr =
            from listLength in Fuzzr.Int(0, 10)
            from enumerable in Fuzzr.Int().Many(listLength)
            select enumerable.ToList();
        return listFuzzr;
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", ListFuzzr())
            from result in Checkr.Act("Result",
                () => TheReverser.Reverses(TheReverser.Reverses(input)))
            from trace in Checkr.Trace("Input ", () => input)
            from traceResult in Checkr.Trace("Result", () => result)
            from expect in Checkr.Expect(
                "Input == Result", () => input.SequenceEqual(result))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Input == Result", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Result", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 42 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Input ", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Result", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("[ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]", article.Execution(1).Trace(2).Read().Value);
    }
}