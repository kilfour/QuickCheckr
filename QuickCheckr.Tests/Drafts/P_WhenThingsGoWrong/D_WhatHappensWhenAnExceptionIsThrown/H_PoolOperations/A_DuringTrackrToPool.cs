using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an excecption occurs during the execution of `Trackr.ToPool` it is considered a failure.
""")]
public class A_DuringTrackrToPool : CheckrTest<A_DuringTrackrToPool>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document(a => a.Run(642999682));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        Trackr.ToPool("Store In Pool", () => ComputerSays.No<Case>("Boom"));

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Boom", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().PoolTraces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Store In Pool", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Case-1", article.Execution(1).PoolTrace(1).Read().Value);
    }
}