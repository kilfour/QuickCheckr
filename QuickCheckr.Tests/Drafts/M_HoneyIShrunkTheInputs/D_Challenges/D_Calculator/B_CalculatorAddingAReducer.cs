using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.D_Calculator;

[DocFile]
[DocContent(
@"And a DeliberationPolicy.")]
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
        Run(376086434);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                (caseFile.InputCount == 0 ? 100 : 0) +
                caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
            DeliberationTarget = 2,
            ReportMode = config.ReportMode & ~ReportMode.StackTrace
        };

    private static int DepthOf(Expr a, int depth = 0)
    {
        if (a is Term term)
            return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
        return depth;
    }

    [CodeSnippet]
    [CodeRemove("CalculatorFuzzr.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from showrTerm in Showr.ForClassification(a => a.For<Term>()
            .Bounds("( ", " )")
            .Delimiter(" ")
            .HidePropertyNames()
            .Property(a => a.L)
            .Property(a => a.Op, a =>
                a switch
                {
                    Operation.Add => "+",
                    Operation.Div => "/",
                    _ => throw new ArgumentOutOfRangeException()
                })
            .Property(a => a.R))
        from showrOp in Showr.ForClassification(a => a.For<Num>()
            .Bounds("", "")
            .Delimiter("")
            .HidePropertyNames())
        from input in Checkr.Input("Calculator", CalculatorFuzzr.TheFuzzr,
            Shrink.When<int>(s => s
                    .As<Num>(a => a.Value)
                    .As<Term, Expr>(a => a.R)
                    .Value(t => t.Op == Operation.Div),
                s => s.Simplify(Reduce.Towards(1))),
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(0))))
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
        Assert.Equal(12, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Evaluate", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Calculator", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( _ / ( 8 / _ ) )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( _ / ( 0 / _ ) )", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Op: Div, L: { Op: Add, L: { Value: 4 }, R: { Value: 6 } }, R: { Op: Div, L: { Value: 8 }, R: { Value: 9 } } }", article.Execution(1).Trace(1).Read().Value);
    }
}
