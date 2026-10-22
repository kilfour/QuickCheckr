using QuickCheckr.Shrinking;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.A_CandidateClassifiers.Methods;

[DocFileHeader("Collection on Shrink")]
public class B1_TryCollectionOnShrink : CheckrTest<B1_TryCollectionOnShrink>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(899182715));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5),
            Shrinkr.For<IEnumerable<int>>().Classify(Try.Collection<int>([0], [1], [42])))
        from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("all answers equal 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("answers", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 2, 80 ]", article.Execution(1).Input(1).Read().Value);
    }

    public class Thing;
}
