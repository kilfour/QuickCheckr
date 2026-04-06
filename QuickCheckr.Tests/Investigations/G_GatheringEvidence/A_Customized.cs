using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.G_GatheringEvidence;

// [DocFile]
public class A_Customized : QCTest<A_Customized>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact(Skip = "Too slow")]
    [DocBoldHeader("The Runner")]
    [DocExample(typeof(A_Customized), nameof(Example))]
    [DocCheckrHeader]
    [DocExample(typeof(A_Customized), nameof(TheCheckr))]
    [DocReportHeader]
    [DocReport]
    [CodeSnippet]
    public override void Example() =>
        TheCheckr().GatherEvidence(
            50.Investigations(),
            10.Runs(),
            1.ExecutionsPerRun(),
            a => a with { FileAs = "GatheringEvidence" },
            a => a.InputsNamed<string>("Input", a => a.IndexOf('a') + (a.Length * 100) + (a.Count(a => a == 'a') * 1000)));

    [CodeSnippet]
    private CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.String(Fuzzr.Char('a', 'c'), 0, 3))
        from expect in Checkr.Expect("does not contain a", () => !input.Contains('a'))
        select Case.Closed;

    protected override void Verify(Article article)
    {
    }
}
