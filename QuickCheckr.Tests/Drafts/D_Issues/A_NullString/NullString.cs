using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.D_Issues.A_NullString;

[DocFile]
public class NullString : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.String())
            from spec in Checkr.Spec("Fails", () => input.Length > 0 && false)
            select Case.Closed;
    }

    [Fact]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(NullString), nameof(GetCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("NullString.txt", "text")]
    [DocContent("We expect 'Input' to be irrelevant because of `&& false` in the spec, but:  ")]
    [DocContent("\n**Investigate.EvidenceFailures:**")]
    [DocCodeFile("evidence-failures.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 1.ExecutionsPerRun(), cfg => cfg with
            {
                // Investigations = [new(() => Write.Here("evidence-failures.txt"), Procedure.EvidenceFailures)],
            }));
        WriteAsserts(article, false);
        WriteReport<NullString>(article, false);
        ExplainMe<NullString>(false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        // WIP
    }
}