using QuickCheckr;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.B_LargeUnionList;

[DocFile]
[DocContent(
@"
Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
")]
public class Z_LargeUnionListTestr : InvariantsTest<Z_LargeUnionListTestr>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocInvariantHeader]
    [DocInvariant]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    [CodeReplace("664059097", "1_000.Runs()")]
    protected override CaseFile TheInvariant() =>
        Testr.Named("No more than four distinct integers.")
            .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
            // Prefer less lists, stop at 1
            .Deliberate(a => a.Count, 1)
            .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4)
            .Run(664059097);

    protected override void Verify(Article article)
    {
        Assert.Equal("No more than four distinct integers.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(11, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _, 66, 42, 89, 25 ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ [ _, 43, 26, 44, 0 ] ]", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ ], [ 31, 86, 93, 91, 95 ], [ 19, 41, 94 ], [ ], [ 6, 48, 40, 26, 99, 87, 45, 68 ], [ 20, 29, 66, 42, 89, 25 ] ]", article.Execution(1).Trace(1).Read().Value);
        Assert.False(article.Execution(1).Trace(1).Read().Labeled);
    }
}