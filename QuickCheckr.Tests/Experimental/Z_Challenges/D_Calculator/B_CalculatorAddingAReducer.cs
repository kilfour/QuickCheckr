using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

[DocFile]
[DocContent(
@"And a DeliberationPolicy.  
The result presented here is ok, but in reality, this sometimes produces non-explanatory results.")]
public class B_CalculatorAddingAReducer : CheckrTest<B_CalculatorAddingAReducer>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1628290528);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
            DeliberationTarget = 1,
            ReportMode = config.ReportMode & ~ReportMode.StackTrace
        };

    private static int DepthOf(Expr a, int depth = 0)
    {
        if (a is Term term)
            return Math.Max(DepthOf(term.Left, depth + 1), DepthOf(term.Right, depth + 1));
        return depth;
    }

    [CodeSnippet]
    [CodeRemove("CalculatorFuzzr.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Calculator", CalculatorFuzzr.TheFuzzr,
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
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
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Evaluate", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Calculator", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Operation: Divide, Right: { Operation: Divide, Left: { Value: 6 }, Right: { Value: 8 } } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ Operation: Divide, Right: { Operation: Divide, Left: { Value: 1 }, Right: { Value: 2 } } }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Operation: Divide, Left: { Operation: Divide, Left: { Value: 6 }, Right: { Value: 5 } }, Right: { Operation: Divide, Left: { Value: 6 }, Right: { Value: 8 } } }", article.Execution(1).Trace(1).Read().Value);
    }
}
