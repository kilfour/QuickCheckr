using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes.ConditionalShrinkers;

[DocFile]
public class OnPath : CheckrTest<OnPath>
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
        Run(1.Runs());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 21)),
            Shrink.When<int>(
                s => s.As<Inter>(a => a.A).Target(),
                s => s.Simplify(Reduce.Towards(3))),
            Shrink.When<int>(
                s => s.As<Inter>(a => a.B).Target(),
                s => s.Simplify(Reduce.Towards(0))))
        from expect in Checkr.Expect("Sum smaller than 11.", () => inter.A + inter.B < 11)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Sum smaller than 11.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Inter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 42, B: 21 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 3, B: 8 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("No witness for reducer at 'Inter.A'.", article.Execution(1).Warning(1).Read().Value);
    }

    public record Inter(int A, int B);
}