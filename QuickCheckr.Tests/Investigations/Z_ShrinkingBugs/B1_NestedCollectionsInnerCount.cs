using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
[DocContent(
@"Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
")]
public class B1_NestedCollectionsInnerCount : CheckrTest<B1_NestedCollectionsInnerCount>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(1134566081);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10),
            Shrink.CollectionFromState<IEnumerable<int>>([[], [], [], [], []], []))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
