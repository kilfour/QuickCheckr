using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

[DocFile]
[DocContent(
@"And a DeliberationPolicy.  
The result presented here is ok, but in reality, this sometimes produces non-explanatory results.")]
public class B_CalculatorAddingAReducer : CheckrTest<B_CalculatorAddingAReducer>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact(Skip = "wip")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1000.Runs());

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
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
        from trace in Checkr.Trace("Original", () => input)
        from eval in Checkr.Act("Evaluate", input.Evaluate)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
