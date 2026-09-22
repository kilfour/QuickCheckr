using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Sub;

[DocFile]
public class A_Default : CheckrTest<A_Default>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(844205832));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from expr in Checkr.Input("Expr", Fuzzr.Constant(
            new Term { L = new Num { Value = 42 }, R = new Num { Value = 0 }, Op = Operation.Div }))
        from trace in Checkr.Trace("Original", a => a.Show(expr))
        from eval in Checkr.Act("Evaluate", expr.Evaluate)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("DivideByZeroException: Attempted to divide by zero.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Evaluate", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Expr", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Op: Div, R: { Value: 0 } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["{ Op: Div, L: { Value: 42 }, R: { Value: 0 } }"], article.Execution(1).Trace(1).Read().Value);
    }
}