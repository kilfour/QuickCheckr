using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Arteries;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
public class C_PassedIsNotTheOppositeOfFailed : CheckrTest<C_PassedIsNotTheOppositeOfFailed>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => true;

    [Fact]
    [DocExample(typeof(Store))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1724204120, 20.ExecutionsPerRun());

    protected override CheckrConfig Configure(CheckrConfig config) => config with { DebugArtery = FileLog.Write("debug.log") };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from store in Trackr.Stashed(() => new Store())
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from name in Checkr.Input("Name", Fuzzr.String())
        from _ in Checkr.OneOfWhen(
            Checkr.Option(() => true,
                from thingId in Checkr.Act("Create", () => store.Add(counter, new Thing()))
                from pool in Trackr.ToPool("To Pool", () => thingId)
                select Case.Closed),
            Trackr.PoolWhen<int>("thing to check", a => a.Count > 0,
                info => Checkr.Expect("thing exists", () => store.GetById(info.Value) is not null)))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    public class Thing
    {
        public int Id { get; set; }
    }

    [CodeExample]
    public class Store
    {
        private readonly List<Thing> things = [];
        private int lastId = 0;
        public int Add(int counter, Thing thing)
        {
            thing.Id = ++lastId;
            if (counter < 2)
                things.Add(thing);
            return thing.Id;
        }

        public Thing? GetById(int id)
            => things.FirstOrDefault(c => c.Id == id);
    }
}
