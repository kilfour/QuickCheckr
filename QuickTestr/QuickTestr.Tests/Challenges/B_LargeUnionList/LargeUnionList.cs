using QuickCheckr;
using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.B_LargeUnionList;

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
public class LargeUnionList : QuickTestrPropertyTest<LargeUnionList>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1575924946")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("No more than four distinct integers.")
            .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
            .Deliberate(a => 0 - a.Count, -5)
            .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4)
            .StoreCaseFiles(journalist)
            .Run(1575924946);

    protected override void Verify(Article article)
    {
        Assert.Equal("No more than four distinct integers.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(20, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _, 17 ], [ 51 ], [ 6 ], [ 74 ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ [ _, 0 ], [ 1 ], [ 2 ], [ 3 ] ]", article.Execution(1).Input(1).Read().Redux.Value);
    }
}