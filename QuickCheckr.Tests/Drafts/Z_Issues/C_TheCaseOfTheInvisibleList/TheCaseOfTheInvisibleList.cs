using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.Z_Issues.C_TheCaseOfTheInvisibleList;

[DocFile]
public class TheCaseOfTheInvisibleList : CheckrTest<TheCaseOfTheInvisibleList>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    [DocContent("Shrinker marks the list as irrelevant, but it is **not**.")]
    public override void Example() =>
        Run(1954145022, 10.ExecutionsPerRun());

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Default };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from thing in Trackr.Stashed(() => new Thing())
        //from shr in Shrinkr.For<IEnumerable<int>>([1])
        from ints in Checkr.Input("ints", Fuzzr.Int().Many(2))
        from trace in Checkr.Trace("ints", () => ints)
        from act in Checkr.Act("change", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("has 1", () => thing.Ints.Contains(1))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    [CodeExample]
    public class Thing { public List<int> Ints = [1]; }
}
