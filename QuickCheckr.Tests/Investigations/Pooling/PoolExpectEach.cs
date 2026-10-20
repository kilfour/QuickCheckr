using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Pooling;

[DocFile]
public class PoolExpectEach : CheckrTest<PoolExpectEach>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(10.ExecutionsPerRun()));

    public class Thing;
    public class OtherThing;

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from sequence in Checkr.Sequence(
            Poolr.Add("1", Checkr.Capture(() => new Thing())),
            Poolr.Add("2", Checkr.Capture(() => new Thing())),
            Poolr.Add("3", Checkr.Capture(() => new OtherThing()))
        )
        from check in Trackr.PoolExpectEach<Thing, OtherThing>(
            "just checking",
            (a, b) => true,
            (a, bs) => bs.Count == 0)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("just checking: Thing-1", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().PoolTraces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("2", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Thing-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("3", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("OtherThing-1", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal("just checking: Thing-1", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);

    }
}
