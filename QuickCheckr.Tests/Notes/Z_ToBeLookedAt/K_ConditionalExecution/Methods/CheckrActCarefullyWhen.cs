using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_ConditionalExecution.Methods;

[DocFile]
public class CheckrActCarefullyWhen : CheckrTest<CheckrActCarefullyWhen>
{
    protected override bool Asserts => false;
    protected override bool Report => true;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(625676917, 3.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
        from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
        from notNull in Checkr.Expect("Not Null", () => thing.HasValue)
        select Case.Closed;


    public class Thing();

    protected override void Verify(Article article)
    {
        Assert.Equal("Not Null", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Not Null", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}