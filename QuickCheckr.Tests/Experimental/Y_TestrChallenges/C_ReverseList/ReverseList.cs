using QuickCheckr;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Expersimental.Z_Challenges.C_ReverseList;

[DocFile]
[DocContent(
@"
This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
")]
public class ReverseList : TestrTest<ReverseList>
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
        Run(12901993);

    protected override ITestrRunner TheTestr() =>
        Testr
            .Named("Reversing a list of integers results in the same list")
            .For(Fuzzr.Int().Many(0, 10).ToList())
            .Assert(a =>
            {
                var reversed = new List<int>(a);
                reversed.Reverse();
                return reversed.SequenceEqual(a);
            });

    protected override void Verify(Article article)
    {
        Assert.Equal("Reversing a list of integers results in the same list", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 94 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ _, 0 ]", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 76, 92, 75, 6, 94 ]", article.Execution(1).Trace(1).Read().Value);
    }
}