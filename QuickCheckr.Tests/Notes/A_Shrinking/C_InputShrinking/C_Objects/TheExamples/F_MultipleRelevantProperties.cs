using QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.Models;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class F_MultipleRelevantProperties : BaseTest
{
    [Fact]
    [DocExample(typeof(F_MultipleRelevantProperties), nameof(GetCheckr))]
    [DocCodeFile("F_MultipleRelevantProperties.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(10000.Runs(), 1.ExecutionsPerRun(), a => a with { Seed = 1889365782 }));
        WriteAsserts(article, true);
        WriteReport<F_MultipleRelevantProperties>(article, true);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from standin in Trackr.StandIn(123)
            from obj in Checkr.Input("BaggyInters", Fuzzr.One<BaggyInters>())
            from trace in Checkr.Trace("Original", () => obj)
            from spec in Checkr.Spec("obj.IntOne == 1 || obj.IntThree == 1",
                () => obj.IntOne != 1 && obj.IntThree != 1)
            select Case.Closed;
    }
}