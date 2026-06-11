using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.A_CandidateClassifiers;
using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.B_PolicyBasedClassifiers;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification;

[DocFile]
[DocLink(typeof(CandidateClassifiers))]
[DocLink(typeof(PolicyBasedClassifiers))]
[DocContent(
@"Classification is done by trying different *candidates* for an input and verifying if the test still fails.  

- If removing/changing the input (via candidates) still reproduces the failure, the input is irrelevant.
- If the failure goes away for at least one candidate, the input is relevant.

So when supplying a custom classifier, you are basically supplying a list of candidates.  
There are various ways to do this :
- [Candidate-based Classifiers][CandidateClassifiers]
- [Policy-based Classifiers][PolicyBasedClassifiers]
")]
public class Classifiers : CheckrTest<Classifiers>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"The following example demonstrates how the various classifiers can be used chained.    
It is a bit of a contrived example as the default classifiers handle this case just fine.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1521363987));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answer in Checkr.Input("answer", Fuzzr.Int(),
            Shrink.OnType<int>(s => s.Classify(Try.Default<int>(), Try.Values(0, 1, 42))))
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