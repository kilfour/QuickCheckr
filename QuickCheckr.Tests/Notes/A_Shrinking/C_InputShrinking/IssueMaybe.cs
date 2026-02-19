using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking;

[DocFile]
public class IssueMaybe : CheckrTest<IssueMaybe>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(346495699);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answers", Fuzzr.Int().Many(1))
        from trace in Checkr.Trace("answers", () => answers)
        from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("all answers equal 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("answers", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("answers", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 15 ]", article.Execution(1).Trace(1).Read().Value); // <= incorrect
    }
}
