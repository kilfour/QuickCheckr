using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.G_GatheringEvidence;

// [DocFile]
public class A_Customized : QCTest<A_Customized>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocBoldHeader("The Runner")]
    [DocExample(typeof(A_Customized), nameof(Example))]
    [DocCheckrHeader]
    [DocExample(typeof(A_Customized), nameof(TheCheckr))]
    [DocReportHeader]
    [DocReport]
    [CodeSnippet]
    public void Example() =>
        TheCheckr().GatherEvidence(
            20.Investigations(),
            20.Runs(),
            1.ExecutionsPerRun(),
            a => a with { FileAs = "GatheringEvidence" },
            a => a.GetInput<string>("Input").Contains('c') ? 1 : 0);

    [CodeSnippet]
    private CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.String(Fuzzr.Char('a', 'c'), 0, 3))
        from expect in Checkr.Expect("does not contain a", () => !input.Contains('a'))
        select Case.Closed;

    protected override void Verify(Article article)
    {
    }
}
