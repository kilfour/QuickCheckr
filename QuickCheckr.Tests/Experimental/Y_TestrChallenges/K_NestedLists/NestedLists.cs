using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_TestrChallenges.K_NestedLists;

[DocFile]
public class NestedLists : TestrTest<NestedLists>
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
        Run(1959968277);

    [CodeSnippet]
    protected override ITestrRunner GetTestr() =>
        Testr
            .Named("The sum of lengths of the element lists is at most 10.")
            .For(Fuzzr.Int().Many(0, 20).ToList().Many(0, 20).ToList())
            .Deliberate(a => a.Count)
            .Assert(a => a.Sum(a => a.Count) <= 10);

    protected override void Verify(Article article)
    {
        Assert.Equal("The sum of lengths of the element lists is at most 10.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(31, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _, _, _, _, _, _, _, _, _, _, _ ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ 11, 46, 66 ], [ 48, 3, 13, 76, 75, 42, 97, 16, 59, 9, 29, 78, 17, 52, 68, 23, 94, 22, 8, 34 ], [ 87, 40, 45, 37, 74, 94, 74, 84, 54, 40, 38, 27, 59 ], [ 2, 23, 80, 49 ], [ 93, 39, 12, 70, 49, 30 ], [ 21, 84, 41, 23, 89, 38, 32, 36, 61, 62, 68, 39, 2, 47, 23 ], [ 6, 69, 91, 51, 58, 30 ], [ 1, 14, 72, 76, 41, 25, 75, 70, 96, 7, 99, 74, 89, 44 ], [ 50, 99, 94, 23, 61, 39 ], [ 41, 48, 84, 70, 8, 7, 1, 57, 78, 17, 90 ], [ 41, 10, 18 ], [ 91, 12, 85, 92, 72, 32, 2, 73, 33, 87, 74, 20 ], [ 56, 35, 69, 28, 45, 72, 24, 92, 70, 17, 69, 69, 39, 29, 83, 84, 72, 4, 22 ] ]", article.Execution(1).Trace(1).Read().Value);
        Assert.False(article.Execution(1).Trace(1).Read().Labeled);
    }
}