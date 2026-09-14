using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.CookBook.A_ListReverse;

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
public class ListReverse : CheckrTest<ListReverse>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [CodeExample]
    public class TheReverser
    {
        public static List<int> Reverses(List<int> list)
            => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];

        private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
            => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
    }

    [Fact]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(ListReverse), nameof(ListFuzzr))]
    [DocBoldHeader("The Checkr")]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(498116108));

    [CodeSnippet]
    private static FuzzrOf<List<int>> ListFuzzr() =>
        from listLength in Fuzzr.Int(0, 10)
        from enumerable in Fuzzr.Int().Many(listLength)
        select enumerable.ToList();

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from showr in Showr.ForInput()
        from input in Checkr.Input("Input", ListFuzzr())
        from result in Checkr.Act("Result",
            () => TheReverser.Reverses(TheReverser.Reverses(input)))
        from traceResult in Checkr.Trace("Result", () => result)
        from expect in Checkr.Expect(
            "Input == Result", () => input.SequenceEqual(result))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input == Result", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Result", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 42 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Result", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]", article.Execution(1).Trace(1).Read().Value);
    }
}