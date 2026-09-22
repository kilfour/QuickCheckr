using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes;

[DocFileHeader("A Second Example")]
public class B_SecondShrinker : CheckrTest<B_SecondShrinker>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(960262083));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answers", Fuzzr.Int().Many(1, 5),
            Shrink.OnType<int>(
                s => s.Classify(Try.Values(42)),
                s => s.Simplify(Reduce.Towards(0))))
        from trace in Checkr.Trace("original", a => a.Show(answers))
        from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("all answers equal 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("answers", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 73 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ 43 ]", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["[ 73 ]"], article.Execution(1).Trace(1).Read().Value);
    }
}
