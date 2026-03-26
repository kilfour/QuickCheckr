using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.C_ReverseList;

[DocFile]
[DocContent(
@"
This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
")]
public class ReverseList : CheckrTest<ReverseList>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(767678629);

    [CodeSnippet]
    private static readonly FuzzrOf<List<int>> TheFuzzr =
         from list in Fuzzr.Int().Many(5)
         select list.ToList();

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("ReverseList", TheFuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from reverse in Checkr.Act("Reverse List",
            () => { var list = new List<int>(input); list.Reverse(); return list; })
        from expect in Checkr.Expect(
            "Reversing a list of integers results in the same list",
            () => reverse.SequenceEqual(input))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Reversing a list of integers results in the same list", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Reverse List", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("ReverseList", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 92 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 68, 89, 98, 88, 92 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
