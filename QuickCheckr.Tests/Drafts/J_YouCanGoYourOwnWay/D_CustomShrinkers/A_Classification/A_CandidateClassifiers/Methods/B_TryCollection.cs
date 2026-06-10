using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.A_CandidateClassifiers.Methods;

[DocFileHeader("Collection")]
[DocContent(
@$"This is very similar to `{nameof(Try)}.{nameof(Try.Values)}`.  
It only exists to avoid generic type soup and easily made mistakes when typing candidates supply for collections.")]
public class B_TryCollection : CheckrTest<B_TryCollection>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(() => TheCheckr().Run(899182715));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5),
            Shrink.OnType<IEnumerable<int>>(s => s.Classify(Try.Collection([0], [1], [42]))))
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
