using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Experimental.C_CheckingTheClock.Chrono;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.C_CheckingTheClock;

[DocFile]
public class CheckingTheClock : CheckrTest<CheckingTheClock>
{
    protected override bool Report => false;
    protected override bool Asserts => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(41897293));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from warmup in Checkr.Perform(() => Wait(0))
        from slow in Checkr.Act("Slow", () => Timr.For(() => Wait(1)))
        from fast in Checkr.Act("Fast", () => Timr.For(() => Wait(1)))
        from expect in Timr.Expect<bool>("Fast is really faster",
            a => a.First(fast).IsFasterThan(slow).ByAtLeast(50))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Fast is really faster", article.FailureDescription());
        Assert.Equal("Delta:       50 ms", article.FailingExpectationMessages()[0]);
        Assert.StartsWith("First took:  00:00:", article.FailingExpectationMessages()[1]);
        Assert.StartsWith("Second took: 00:00:", article.FailingExpectationMessages()[2]);
        Assert.Equal("-----------------------------", article.FailingExpectationMessages()[3]);
        Assert.StartsWith("Missed by:   00:00:", article.FailingExpectationMessages()[4]);
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Fast", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Slow", article.Execution(1).Action(2).Read().Label);
    }

    private static async Task<bool> Wait(int milliseconds)
    {
        await Task.Delay(milliseconds);
        return true;
    }
}