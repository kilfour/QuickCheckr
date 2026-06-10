using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.A_DuringAct;

[DocFile]
[DocContent("""
Whenever an excecption occurs during the execution of an `Act` it is considered a failure.
""")]
public class A_DuringAct : CheckrTest<A_DuringAct>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(() => TheCheckr().Run(1.Runs(), 2.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Counter", Fuzzr.Counter("cnt"))
        from expect in Checkr.Act("Sometimes throws", () =>
        {
            if (input > 0)
                ComputerSays.No("Only one is available");
        })
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Sometimes throws", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Value);
    }
}