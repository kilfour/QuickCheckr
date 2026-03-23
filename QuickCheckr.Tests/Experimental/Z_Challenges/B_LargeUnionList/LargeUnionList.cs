using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
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
public class LargeUnionList : CheckrTest<LargeUnionList>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1284201255);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                caseFile.InputsNamed<List<List<int>>>("LargeUnionList", a => 0 - a.Count),
            DeliberationTarget = -5
        };

    [CodeSnippet]
    private static readonly FuzzrOf<List<int>> InnerFuzzr =
         from list in Fuzzr.Int().Many(0, 10)
         select list.ToList();

    [CodeSnippet]
    private static readonly FuzzrOf<List<List<int>>> TheFuzzr =
         from list in InnerFuzzr.Many(5, 20)
         select list.ToList();

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("LargeUnionList", TheFuzzr,
                Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "No more than four distinct integers.",
            () => input.SelectMany(a => a).Distinct().Count() <= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("No more than four distinct integers.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(25, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("LargeUnionList", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _ ], [ 98 ], [ 33 ], [ 41 ], [ 73 ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ [ _ ], [ 0 ], [ 1 ], [ 2 ], [ 3 ] ]", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ 37, 88, 82, 64, 26, 15, 91, 65 ], [ 76, 17, 18, 41, 82, 29, 89, 80, 24 ], [ 74, 39, 69, 94, 76, 44, 14, 66 ], [ 32, 98 ], [ 16, 25, 11, 59, 67, 74, 9 ], [ 63, 55, 4 ], [ ], [ 96 ], [ 16, 42, 86 ], [ 92, 57, 74, 11, 38, 67 ], [ 7, 21, 13, 49, 72, 34, 88, 10, 1, 92 ], [ 24, 61, 16 ], [ 96, 12, 20, 36, 67, 80, 80 ], [ 98 ], [ 33, 73 ], [ ], [ 41 ], [ 73 ] ]", article.Execution(1).Trace(1).Read().Value);
    }
}