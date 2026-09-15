using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;

public class D_AddingAnotherInput : CheckrTest<D_AddingAnotherInput>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("Now, if we add another input to the Checkr, one which does not influence the failure.")]
    [DocCheckr]
    [DocContent("The following happens.")]
    [DocReport]
    [DocContent(
@"That's basically the same thing as before, because **QuickCheckr does not report irrelevant inputs**.
> This behaviour is key. 
> QuickCheckr actively removes noise from the failing case.  
> => Less cognitive load. More signal.")]
    public override void Example() => Document(a => a.Run(127648209));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int())
        from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
        from expect in Checkr.Expect("Input /= 42", () => input != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input /= 42", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }
}