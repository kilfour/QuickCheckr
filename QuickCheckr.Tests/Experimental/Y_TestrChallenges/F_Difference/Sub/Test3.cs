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
Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
")]
public class Test3 : TestrTest<Test3>
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
        Run(1231462692);

    [CodeSnippet]
    [CodeRemove("Difference.")]
    [CodeRemove("The.")]
    protected override ITestrRunner GetTestr() =>
        Testr.Named("Difference is not exactly 1.")
            .For(Difference.TheFuzzr, Difference.The.Shrinkr)
            .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1);

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference is not exactly 1.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 51, B: 52 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 9 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: 51, B: 52 }", article.Execution(1).Trace(1).Read().Value);
        Assert.False(article.Execution(1).Trace(1).Read().Labeled);
    }
}