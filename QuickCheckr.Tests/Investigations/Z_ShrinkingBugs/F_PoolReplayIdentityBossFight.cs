using System.Diagnostics;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Diagnostics;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
public class F_PoolReplayIdentityBossFight : CheckrTest<F_PoolReplayIdentityBossFight>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => true;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocExample(typeof(OtherThing))]
    [DocExample(typeof(ThingFactory))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        //Document(a => a.Run(769948390, 6.ExecutionsPerRun()));
        Document(a => a.Autopsy(769948390, 6.ExecutionsPerRun(),
            AutopsyProbe.ExecutionShrinking
                .On(() => Debugger.Break(),
                    AutopsyOf.ExecutionShrinking.Pass(1).Checking(2).Executing(4),
                    AutopsyOf.ExecutionShrinking.Pass(1).Checking(2).Executing(6))));

    public record ThingInfo(int ThingId);
    public record OtherThingInfo(int OtherThingId);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from factory in Trackr.Stashed(() => new ThingFactory())
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from op in Checkr.OneOfWhen(
            Checkr.Option(() => counter == 1,
                from thing in Checkr.Act("Create Thing 1", factory.CreateThing)
                from pool in Trackr.ToPool("Thing", () => new ThingInfo(thing.Id))
                select Case.Closed),

            Checkr.Option(() => counter == 2,
                from thing in Checkr.Act("Create Other Thing 1", factory.CreateOtherThing)
                from pool in Trackr.ToPool("Other Thing", () => new OtherThingInfo(thing.Id))
                select Case.Closed),

            Checkr.Option(() => counter == 3,
                from thing in Checkr.Act("Create Other Thing 2", factory.CreateOtherThing)
                from pool in Trackr.ToPool("Other Thing", () => new OtherThingInfo(thing.Id))
                select Case.Closed),

            Checkr.Option(() => counter == 4,
                Trackr.OneOfPool<OtherThingInfo>("Update Other Thing", thing =>
                    Checkr.Act("Update", () => { factory.GetOtherThingById(thing.Value.OtherThingId).Extra = 42; }))),

            Checkr.Option(() => counter == 5,
                Trackr.OneOfPoolWhen<ThingInfo, OtherThingInfo>(
                    "Assignable Pair",
                    (thing, otherThing) =>
                        factory.GetThingById(thing.ThingId)
                            .CanAssignTo(factory.GetOtherThingById(otherThing.OtherThingId)),
                    (thing, otherThing) =>
                        Checkr.Act("Assign",
                            () =>
                                factory.GetOtherThingById(otherThing.Value.OtherThingId)
                                    .Assign(factory.GetThingById(thing.Value.ThingId))))),

            Checkr.Option(() => counter == 6,
                Trackr.PoolExpectEach<OtherThingInfo>("Assigned to 42",
                    otherThingInfo =>
                    {
                        var otherThing = factory.GetOtherThingById(otherThingInfo.OtherThingId);
                        return otherThing.Assigned ? otherThing.ThingId == 2 : true;
                    }))
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {

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
