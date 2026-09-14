using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.C_ReverseList;

[DocFile]
[DocContent(
@"
This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
")]
public class ReverseList : QuickTestrPropertyTest<ReverseList>
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
    [CodeRemove("12901993")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Reversing a list of integers results in the same list")
            .For(Fuzzr.Int().Many(0, 10).ToList())
            .Assert(a =>
            {
                var reversed = new List<int>(a);
                reversed.Reverse();
                return reversed.SequenceEqual(a);
            })
            .StoreCaseFiles(journalist)
            .Run(12901993);

    protected override void Verify(Article article)
    {
        Assert.Equal("Reversing a list of integers results in the same list", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 94 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ _, 0 ]", article.Execution(1).Input(1).Read().Redux.Value);
    }
}