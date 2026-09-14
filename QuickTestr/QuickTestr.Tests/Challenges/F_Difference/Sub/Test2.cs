using QuickTestr.Tests.Tools;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
")]
public class Test2 : QuickTestrPropertyTest<Test2>
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
    [CodeRemove("1449083695")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Difference must not be small.")
            .For(Difference.TheFuzzr, Difference.The.Shrinkr)
            .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B)
            .StoreCaseFiles(journalist)
            .Run(1449083695);

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference must not be small.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 37, B: 38 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 6 }", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
