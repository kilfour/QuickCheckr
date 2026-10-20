using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.T_PoolReplay;

[DocFile]
public class F_PoolReplayIdentityBossFight : CheckrTest<F_PoolReplayIdentityBossFight>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocExample(typeof(OtherThing))]
    [DocExample(typeof(ThingFactory))]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(769948390, 6.ExecutionsPerRun()));

    public record ThingInfo(int ThingId);
    public record OtherThingInfo(int OtherThingId);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from factory in Trackr.Stashed(() => new ThingFactory())
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from op in Checkr.OneOfWhen(
            Checkr.Option(() => counter == 1,
                from thing in Checkr.Act("Create Thing 1", factory.CreateThing)
                from pool in Poolr.Add("Thing", Checkr.Capture(() => new ThingInfo(thing.Id)))
                select Case.Closed),

            Checkr.Option(() => counter == 2,
                from thing in Checkr.Act("Create Other Thing 1", factory.CreateOtherThing)
                from pool in Poolr.Add("Other Thing", Checkr.Capture(() => new OtherThingInfo(thing.Id)))
                select Case.Closed),

            Checkr.Option(() => counter == 3,
                from thing in Checkr.Act("Create Other Thing 2", factory.CreateOtherThing)
                from pool in Poolr.Add("Other Thing", Checkr.Capture(() => new OtherThingInfo(thing.Id)))
                select Case.Closed),

            Checkr.Option(() => counter == 4,
                Trackr.OneOfPool<OtherThingInfo>("Update Other Thing", thing =>
                    Checkr.Act("Update", () => { factory.GetOtherThingById(thing.Value.OtherThingId).Extra = 42; }))),

            Checkr.Option(() => counter == 5,
                from pair in Poolr.OneOfWhen<ThingInfo, OtherThingInfo>(
                    "Assignable Pair",
                    (thing, otherThing) =>
                        factory.GetThingById(thing.ThingId)
                            .CanAssignTo(factory.GetOtherThingById(otherThing.OtherThingId)))
                from assign in Checkr.Act("Assign",
                    () => factory.GetOtherThingById(pair.Second.OtherThingId)
                        .Assign(factory.GetThingById(pair.First.ThingId)))
                select Case.Closed),
            Checkr.Option(() => counter == 6,
                from otherThings in Poolr.AllElements<OtherThingInfo>()
                from check in Checkr.ExpectWhen("Assigned to 42",
                    () => otherThings.Count != 0,
                    context =>
                    {
                        var failures = otherThings
                            .Where(element =>
                            {
                                var otherThing = factory.GetOtherThingById(element.Value.OtherThingId);
                                return otherThing.Assigned && otherThing.ThingId != 2;
                            })
                            .Select(element => $"OtherThingInfo-{element.Id}")
                            .ToArray();
                        return failures.Length == 0
                            ? context.Pass()
                            : context.Fail(failures);
                    })
                select Case.Closed)
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Assigned to 42", article.FailureDescription());
        Assert.Equal("OtherThingInfo-2", article.FailingExpectationMessages()[0]);
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(5, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(5, article.Total().Inputs());
        Assert.Equal(4, article.Total().PoolTraces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Thing 1", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Thing", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("ThingInfo-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Other Thing 2", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Other Thing", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("OtherThingInfo-2", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(4, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("4", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Update Other Thing", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("OtherThingInfo-2", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal(5, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Assign", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("5", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Assignable Pair", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("( ThingInfo-1, OtherThingInfo-2 )", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal(6, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(5).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(5).Input(1).Read().Value);
    }

    [CodeExample]
    public class Thing(int id)
    {
        public int Id { get; set; } = id;
        public bool CanAssignTo(OtherThing otherThing)
        {
            return otherThing.Extra == 42;
        }
    }

    [CodeExample]
    public class OtherThing(int id)
    {
        public int Id { get; set; } = id;
        public int Extra { get; set; }
        public bool Assigned => ThingId != 0;
        public int ThingId { get; private set; }
        public void Assign(Thing thing)
        {
            ThingId = thing.Id;
        }
    }

    [CodeExample]
    public class ThingFactory
    {
        public List<Thing> things = [];
        private int nextThingId;
        public Thing CreateThing()
        {
            var thing = new Thing(++nextThingId);
            things.Add(thing);
            return thing;
        }
        public Thing GetThingById(int id) => things.Single(a => a.Id == id);

        public List<OtherThing> otherThings = [];
        private int nextOtherThingId;
        public OtherThing CreateOtherThing()
        {
            var otherThing = new OtherThing(++nextOtherThingId);
            otherThings.Add(otherThing);
            return otherThing;
        }
        public OtherThing GetOtherThingById(int id) => otherThings.Single(a => a.Id == id);
    }
}
