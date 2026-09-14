using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.G_GatheringEvidence;

[DocFile]
public class A_Customized : CheckrTest<A_Customized>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact(Skip = "non deterministic by nature")]
    [DocContent("Test skipped: non deterministic by nature")]
    [DocBoldHeader("The Runner")]
    [DocExample(typeof(A_Customized), nameof(Example))]
    [DocExample(typeof(A_Customized), nameof(TheCheckr))]
    [DocReport]
    [CodeSnippet]
    public override void Example() =>
        Document(a => a
            .Configure(a => a with { FileAs = "Gathering Evidence" })
            .Conduct(
                20.Investigations(),
                20.Runs(),
                1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.String(Fuzzr.Char('a', 'c'), 0, 3))
        from deliberate in Checkr.Deliberate(() => input.Contains('c') ? 1 : 0)
        from expect in Checkr.Expect("does not contain a", () => !input.Contains('a'))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("does not contain a", article.PassedExpectation(1).Read().Label);
        Assert.Equal(32, article.PassedExpectation(1).Read().TimesPassed);
    }
}
