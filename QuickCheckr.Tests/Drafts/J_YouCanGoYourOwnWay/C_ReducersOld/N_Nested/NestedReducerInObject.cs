using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.C_ReducersOld.N_Nested;

[DocFile]
public class NestedReducerInObject : CheckrTest<NestedReducerInObject>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [CodeExample]
    public class TheUniverse
    {
        public int Answer { get; set; }
    }

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(TheUniverse))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() => Run(686899245);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Reduce.TowardsZero)
        from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("answer <= 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Answer: 61 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ Answer: 43 }", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
