using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBdugs;

[DocFile]
public class E_PoolReplayIdentityComplex : CheckrTest<E_PoolReplayIdentityComplex>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
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
                from pool in Trackr.ToPool("Thing", () => thing.Id)
                select Case.Closed),

            Checkr.Option(() => counter == 2,
                from thing in Checkr.Act("Create Thing 2", factory.Create)
                from pool in Trackr.ToPool("Thing", () => thing.Id)
                select Case.Closed),

            Checkr.Option(() => counter == 3,
                Trackr.OneOfPool<int>("Update Thing", a => Checkr.Act("", () => { factory.GetById(a.Value).Extra = 42; }))),

            Checkr.Option(() => counter == 4,
                Trackr.OneOfPool<int>("Use Thing", thing =>
                    from trace in Checkr.Trace("Pool Element", () => $"{thing.Id}:{thing.Value}")
                    from expect in Checkr.Expect(
                        "Second pool element fails",
                        () => !(factory.GetById(thing.Value).Extra == 42))
                    select Case.Closed)))
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