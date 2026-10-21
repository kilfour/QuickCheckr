using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.T_PoolReplay;

[DocFile]
public class E_PoolReplayIdentityComplex : CheckrTest<E_PoolReplayIdentityComplex>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(204955625, 4.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from factory in Trackr.Stashed(() => new ThingFactory())
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from op in Checkr.OneOfWhen(
            Checkr.Option(() => counter == 1,
                from thing in Checkr.Act("Create Thing 1", factory.Create)
                from pool in Poolr.Add("Thing", Checkr.Capture(() => thing.Id))
                select Case.Closed),

            Checkr.Option(() => counter == 2,
                from thing in Checkr.Act("Create Thing 2", factory.Create)
                from pool in Poolr.Add("Thing", Checkr.Capture(() => thing.Id))
                select Case.Closed),

            Checkr.Option(() => counter == 3,
                from thing in Poolr.OneOf<int>("Update Thing")
                from update in Checkr.Act("", () => { factory.GetById(thing).Extra = 42; })
                select Case.Closed),

            Checkr.Option(() => counter == 4,
                from thing in Poolr.OneOf<int>("Use Thing")
                from poolId in PoolIdentity.Of(thing)
                from trace in Checkr.Trace("Pool Element", _ => [$"{poolId}:{thing}"])
                from expect in Checkr.Expect(
                    "Second pool element fails",
                    () => !(factory.GetById(thing).Extra == 42))
                select Case.Closed))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    public class ThingFactory
    {
        public List<Thing> things = [];
        private int next;
        public Thing Create()
        {
            var thing = new Thing(++next);
            things.Add(thing);
            return thing;
        }
        public Thing GetById(int id) => things.Single(a => a.Id == id);
    }

    public record Thing(int Id) { public int Extra { get; set; } };
}
