using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.J_Distinct;

[DocFile]
[DocContent(
@"
This tests the example provided for the property ""a list of integers containing at least three distinct elements"".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for ""a list of distinct elements"",
so the ""example of size at least N"" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
")]
public class Distinct : CheckrTest<Distinct>
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
        Run(767903368);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Distinct", Fuzzr.Int(1, 10).Many(3, 20),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.ExpectWhen(
            "Contains at least three distinct elements.",
            () => input.Count() >= 3,
            () => input.ToHashSet().Count >= 3)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Contains at least three distinct elements.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Distinct", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 8, 8 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 5, 8, 8 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
