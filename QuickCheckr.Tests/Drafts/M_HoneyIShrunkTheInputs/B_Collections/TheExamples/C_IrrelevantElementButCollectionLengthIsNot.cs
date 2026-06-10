using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;


[DocFile]
public class C_IrrelevantElementButCollectionLengthIsNot : CheckrTest<C_IrrelevantElementButCollectionLengthIsNot>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the collection,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(() => TheCheckr().Run(1393576429));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
        from trace in Checkr.Trace("Original List", () => list)
        from expect in Checkr.Expect("Length <= 2", () => list.Count() <= 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Length <= 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 12, 45, 56, 18, 31 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
