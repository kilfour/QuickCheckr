using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Collapsing;

public class B_UsingDeliberation : CheckrTest<B_UsingDeliberation>
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
        Document(a => a.Run(100.Runs()));

    private int GetDepth(Lister lister)
    {
        if (lister.Child is not null)
            return 1 + GetDepth(lister.Child);
        return 1;
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from lister in Checkr.Input("lister", ListerFuzzr)
        from deliberation in Checkr.Deliberate(() => GetDepth(lister))
        from until in Checkr.DeliberateUntil(1)
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

