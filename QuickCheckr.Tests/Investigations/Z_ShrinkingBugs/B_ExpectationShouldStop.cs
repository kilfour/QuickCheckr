using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
[DocContent("Fixed this one by adding TheCatcher.InTheCheckr to Trackr.ToPool.")]
public class B_ExpectationShouldStop : CheckrTest<B_ExpectationShouldStop>
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
        Document(a => a.Run(1.ExecutionsPerRun()));

    public class Thing { public int Answer { get; set; } = 42; }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Thing", Fuzzr.Constant<Thing>(null!))
        from isnotnull in Checkr.Expect("Is not null", () => input is not null)
        from answer in Trackr.ToPool("To Pool: input.Answer", () => input.Answer)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
