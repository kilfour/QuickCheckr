using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking;

[DocFile]
[DocContent(
@"This is the third step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.")]
public class InputShrinking
{
    private readonly bool WriteReportToDisk = false;

    [Fact]
    [DocContent("The following simple example has one input but always fails.")]
    [DocExample(typeof(InputShrinking), nameof(Example_Check))]
    [DocContent(
@"<Explanation goes here>

So after Input Shrinking, the result is:
")]
    [DocCodeFile("InputShrinking.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(ExampleTest);
        Assert.Equal("Always Fails", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.Total().Inputs()); // should be zero
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);


        var report = TheClerk.Transcribes(article.CaseFile);
        var reader = LinesReader.FromText(report);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Test:                    ExampleTest", reader.NextLine());
        Assert.Contains(" Location: ", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Seed:                    165641598", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (1)", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.Equal("  !! Spec Failed: Always Fails", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
        if (WriteReportToDisk)
            WriteReport.ToFile("InputShrinking.txt", report);
    }

    private static void ExampleTest()
    {
        Example_Check().Run(
            1.Runs(),
            50.ExecutionsPerRun(),
            a => a with { Seed = 165641598 });
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
            from counter in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from spec in Checkr.Spec("Always Fails", () => false)
            select Case.Closed;
    }
}
