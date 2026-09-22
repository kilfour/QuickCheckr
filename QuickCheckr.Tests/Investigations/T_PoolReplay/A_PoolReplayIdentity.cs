using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.T_PoolReplay;

[DocFile]
public class A_PoolReplayIdentity : CheckrTest<A_PoolReplayIdentity>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(100.Runs(), 3.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from factory in Trackr.Stashed(() => new ThingFactory())
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from op in Checkr.OneOfWhen(
            Checkr.Option(() => counter == 1,
                from thing in Checkr.Act("Create Thing 1", factory.Create)
                from pool in Trackr.ToPool("Thing", () => thing)
                select Case.Closed),

            Checkr.Option(() => counter == 2,
                from thing in Checkr.Act("Create Thing 2", factory.Create)
                from pool in Trackr.ToPool("Thing", () => thing)
                select Case.Closed),

            Checkr.Option(() => counter == 3,
                Trackr.OneOfPool<Thing>("Use Thing", thing =>
                    from trace in Checkr.Trace("Pool Element", _ => [$"{thing.Id}:{thing.Value.Id}"])
                    from expect in Checkr.Expect(
                        "Second pool element fails",
                        () => thing.Id != 2)
                    select Case.Closed)))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Second pool element fails", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(2, article.Total().PoolTraces());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Thing 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Thing", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Thing-2", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Use Thing", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("Thing-2", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal("Pool Element", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal(["2:1"], article.Execution(2).Trace(1).Read().Value);
    }

    public class ThingFactory
    {
        private int next;
        public Thing Create() => new(++next);
    }
    public record Thing(int Id);
}
