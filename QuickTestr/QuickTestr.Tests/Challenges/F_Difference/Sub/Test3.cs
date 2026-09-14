using QuickTestr.Tests.Tools;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
")]
public class Test3 : QuickTestrPropertyTest<Test3>
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
    [CodeRemove("Difference.")]
    [CodeRemove("The.")]
    [CodeRemoveJournalist]
    [CodeRemove("1231462692")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Difference is not exactly 1.")
            .For(Difference.TheFuzzr, Difference.The.Shrinkr)
            .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1)
            .StoreCaseFiles(journalist)
            .Run(1231462692);

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference is not exactly 1.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 51, B: 52 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 9 }", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
