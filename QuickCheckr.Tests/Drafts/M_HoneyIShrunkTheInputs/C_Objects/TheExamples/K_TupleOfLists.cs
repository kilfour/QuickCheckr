using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class K_TupleOfLists : CheckrTest<K_TupleOfLists>
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
        Run(1892439403);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Tuple",
            from a in Fuzzr.Int().Many(2)
            from b in Fuzzr.Int().Many(2)
            from c in Fuzzr.Int().Many(0)
            select (a, b, c),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from irrelevant in Checkr.Input("Irrelevant",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            select (a, b))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("First list does not contain 42.", () => !input.a.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}