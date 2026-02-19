using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.D_CustomShrinkers.Including;


[DocContent(
@"Classification is done by trying different *candidates* for an input and verifying if the test still fails.  

- If removing/changing the input (via candidates) still reproduces the failure, the input is irrelevant.
- If the failure goes away for at least one candidate, the input is relevant.

So when supplying a custom classifier, you are basically supplying a list of candidates.  
There are various ways to do this, but here is a first example:
")]
public class A_ASimpleClassifier : CheckrTest<A_ASimpleClassifier>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent("It's a bit of a contrived example as the default classifiers handle this case just fine, but it's here for demonstration purposes.")]
    public override void Example() =>
        Run(1521363987);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answer in Checkr.Input("answer", Fuzzr.Int(), Try.Values(0, 1, 42))
        from expect in Checkr.Expect("answer == 42", () => answer == 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("answer == 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("answer", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("63", article.Execution(1).Input(1).Read().Value);
    }
}