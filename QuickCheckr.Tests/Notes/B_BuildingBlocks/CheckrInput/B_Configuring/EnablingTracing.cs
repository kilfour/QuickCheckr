using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Notes.B_BuildingBlocks.CheckrInput.B_Configuring;

[DocFile]
public class EnablingTracing
{
    [Fact]
    //[DocContent("The following simple example fails on the third execution.")]
    [DocExample(typeof(EnablingTracing), nameof(Example_Check))]

    [DocCodeFile("EnablingTracing.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(ExampleTest);
        Assert.Equal("Always Fails", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);
        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("Int Input", input.Label);
        Assert.Equal("27", input.Value);
        var trace = article.Execution(1).Trace(1).Read();
        Assert.Equal("Int Input", trace.Label);
        Assert.Equal("27", trace.Value);
        trace = article.Execution(1).Trace(2).Read();
        Assert.Equal("Irrelevant", trace.Label);
        Assert.Equal("31", trace.Value);

        var report = TheClerk.Transcribes(article.CaseFile);
        var reader = LinesReader.FromText(report);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Test:                    ExampleTest", reader.NextLine());
        Assert.Contains(" Location: ", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Seed:                    2030851905", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (1):", reader.NextLine());
        Assert.Equal("   - Input: Int Input = 27", reader.NextLine());
        Assert.Equal("   - Trace: Int Input = 27", reader.NextLine());
        Assert.Equal("   - Trace: Irrelevant = 31", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.Equal("  !! Spec Failed: Always Fails", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
        WriteReport.ToFile("EnablingTracing.txt", report);
    }

    private static void ExampleTest()
    {
        Example_Check().Run(a => a with { Seed = 2030851905 });
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
        from intInput in Checkr.Input("Int Input", Fuzzr.Int(), a => a.Trace())
        from traced in Checkr.Input("Irrelevant", Fuzzr.Int(), a => a.Trace())
        from spec in Checkr.Spec("Always Fails", () => intInput < 0)
        select Case.Closed;
        // The default int Fuzzr generates values in the range [1,100)
        // So `intInput < 0` always fails
    }
}