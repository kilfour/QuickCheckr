using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Collapsing;

public class A_SketchingThingsOut : CheckrTest<A_SketchingThingsOut>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1374960450));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from lister in Checkr.Input("lister", ListerFuzzr)
        from expect in Checkr.Expect("no list sum > 100 && count >= 2", () => CheckLister(lister))
        select Case.Closed;

    private static readonly FuzzrOf<Lister> ListerFuzzr =
        from depth in Configr<Lister>.Depth(1, 5)
        from setValues in Configr<Lister>.Property(a => a.Values, Fuzzr.Int().Many(0, 5))
        from lister in Fuzzr.One<Lister>()
        select lister;

    private static bool CheckLister(Lister list)
    {
        if (list.Values.Count >= 2 && list.Values.Sum() > 100)
            return false;
        if (list.Child is not null)
            return CheckLister(list.Child);
        return true;
    }

    protected override void Verify(Article article)
    {

    }

    public class Lister
    {
        public List<int> Values { get; set; } = [];
        public Lister? Child { get; set; }
    }
}

