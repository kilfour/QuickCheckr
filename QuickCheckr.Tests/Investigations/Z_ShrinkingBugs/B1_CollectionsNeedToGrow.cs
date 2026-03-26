using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
[DocContent(
@"Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
")]
public class B1_CollectionsNeedToGrow : CheckrTest<B1_CollectionsNeedToGrow>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(1134566081);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from fuzzr in Checkr.Capture(() => Fuzzr.Int().Many(3, 10))
        from input in Checkr.Input("List", fuzzr,
            // Refuzzing can grow the list.
            // Supplying values *manually* also.
            Shrink.OnType<IEnumerable<int>>(s => s.Classify(Try.Fuzzr(fuzzr, 100))))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count >= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 87, 79, 83 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 87, 79, 83 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
