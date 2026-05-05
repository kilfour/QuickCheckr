using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.A_Primitives.TheExamples;

[DocFile]
public class C_NullableTypes : CheckrTest<C_NullableTypes>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(393010076);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Constant<Nullable<int>>(null!))
        from expect in Checkr.Expect("Fails", () => input.HasValue)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(1).Read().Value);
    }
}