using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class Q_ObjectOfLists : CheckrTest<Q_ObjectOfLists>
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
        Run(1000.Runs());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Museum",
            from a in Fuzzr.Int().Many(2).ToList()
            from b in Fuzzr.Int().Many(2).ToList()
            from c in Fuzzr.Int().Many(0).ToList()
            select new Museum(a, b, c),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from irrelevant in Checkr.Input("Irrelevant",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            select (a, b))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("First list does not contain 42.", () => !input.A.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    public record Museum(List<int> A, List<int> B, List<int> C);
}