using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

[DocFile]
[DocContent(
@"
The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
")]
public class A_Calculator : CheckrTest<A_Calculator>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocFuzzrHeader]
    [DocExample(typeof(CalculatorFuzzr), nameof(CalculatorFuzzr.TheFuzzr))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1264796639);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };

    [CodeSnippet]
    [CodeRemove("CalculatorFuzzr.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Calculator", CalculatorFuzzr.TheFuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from eval in Checkr.Act("Evaluate", input.Evaluate)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("DivideByZeroException: Attempted to divide by zero.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Evaluate", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Calculator", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Op: Div, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }", article.Execution(1).Trace(1).Read().Value);
    }
}
