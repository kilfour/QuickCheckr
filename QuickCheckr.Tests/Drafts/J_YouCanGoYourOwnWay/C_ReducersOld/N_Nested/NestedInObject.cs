using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.C_ReducersOld.N_Nested;

[DocFile]
public class NestedInObject : CheckrTest<NestedInObject>
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
    public override void Example() => Run(1930190827);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answer in Checkr.Input("Answer", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
        from universe in Checkr.Capture(() => new TheUniverse { Answer = answer })
        from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("answer <= 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Answer", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("96", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("43", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
