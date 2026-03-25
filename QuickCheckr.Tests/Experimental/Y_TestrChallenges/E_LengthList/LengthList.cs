using QuickCheckr;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_TestrChallenges.E_LengthList;

[DocFile]
[DocContent(
@"
A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
")]
public class LengthList : TestrTest<LengthList>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(357470573);

    [CodeSnippet]
    protected override ITestrRunner GetTestr() =>
        Testr.Named("The maximum value of the list is smaller than 900.")
            .For(
                from length in Fuzzr.Int(1, 100)
                from list in Fuzzr.Int(0, 1000).Many(length)
                select list.ToList())
            .Assert(a => a.Max() < 900);

    protected override void Verify(Article article)
    {
        Assert.Equal("The maximum value of the list is smaller than 900.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(14, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 905 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ 900 ]", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 176, 244, 257, 301, 257, 329, 381, 764, 442, 261, 535, 550, 677, 905 ]", article.Execution(1).Trace(1).Read().Value);
        Assert.False(article.Execution(1).Trace(1).Read().Labeled);
    }
}