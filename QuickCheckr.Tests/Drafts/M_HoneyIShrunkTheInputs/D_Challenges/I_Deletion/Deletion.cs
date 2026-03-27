using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.I_Deletion;

[DocFile]
[DocContent(
@"
This tests the property ""if we remove an element from a list, the element is
no longer in the list"". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
")]
public class Deletion : CheckrTest<Deletion>
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
        Run(613616447);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Deletion", Fuzzr.Int(1, 10).Many(3, 20))
        from toRemove in Checkr.Input("Element To Remove", () => Fuzzr.OneOf(input))
        from removed in Checkr.Act("Remove Element", () =>
        {
            var list = input.ToList();
            list.Remove(toRemove);
            return list;
        })
        from expect in Checkr.Expect("Element is no longer in the list", () => !removed.Contains(toRemove))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Element is no longer in the list", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        // Assert.Equal(16, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Remove Element", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Deletion", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 3, 3 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Element To Remove", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(2).Read().Value);
    }
}
