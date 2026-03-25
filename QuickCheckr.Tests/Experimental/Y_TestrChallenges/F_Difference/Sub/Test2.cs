using QuickCheckr;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_TestrChallenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
")]
public class Test2 : TestrTest<Test2>
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
        Run(1449083695);

    [CodeSnippet]
    [CodeRemove("Difference.")]
    [CodeRemove("The.")]
    protected override ITestrRunner GetTestr() =>
        Testr.Named("Difference must not be small.")
            .For(Difference.TheFuzzr, Difference.The.Shrinkr)
            .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B);

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference must not be small.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 37, B: 38 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 6 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: 37, B: 38 }", article.Execution(1).Trace(1).Read().Value);
        Assert.False(article.Execution(1).Trace(1).Read().Labeled);
    }
}