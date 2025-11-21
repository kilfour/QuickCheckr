using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class A_FailsOnSize
{
    [Fact]
    [DocExample(typeof(A_FailsOnSize), nameof(Example_Check))]
    [DocCodeFile("A_FailsOnSize.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(ExampleTest);
        Assert.Equal("List does not contain 7", article.FailedSpec());
        // Assert.Equal(1, article.Total().Executions());
        // Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(2, article.Total().Traces());
        // var execution = article.Execution(1).Read();
        // Assert.Equal(1, execution.ExecutionId);
        // var input = article.Execution(1).Input(1).Read();
        // Assert.Equal("Int Input", input.Label);
        // Assert.Equal("27", input.Value);
        // var trace = article.Execution(1).Trace(1).Read();
        // Assert.Equal("Int Input", trace.Label);
        // Assert.Equal("27", trace.Value);
        // trace = article.Execution(1).Trace(2).Read();
        // Assert.Equal("Irrelevant", trace.Label);
        // Assert.Equal("31", trace.Value);

        var report = TheClerk.Transcribes(article.CaseFile);
        var reader = LinesReader.FromText(report);
        // Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        // Assert.Equal(" Test:                    ExampleTest", reader.NextLine());
        // Assert.Contains(" Location: ", reader.NextLine());
        // Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        // Assert.Equal(" Seed:                    2030851905", reader.NextLine());
        // Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        // Assert.Equal("  Executed (1):", reader.NextLine());
        // Assert.Equal("   - Input: Int Input = 27", reader.NextLine());
        // Assert.Equal("   - Trace: Int Input = 27", reader.NextLine());
        // Assert.Equal("   - Trace: Irrelevant = 31", reader.NextLine());
        // Assert.Equal(" ================================", reader.NextLine());
        // Assert.Equal("  !! Spec Failed: Always Fails", reader.NextLine());
        // Assert.Equal(" ================================", reader.NextLine());
        // Assert.True(reader.EndOfContent());
        WriteReport.ToFile("A_FailsOnSize.txt", report);
    }

    private static void ExampleTest()
    {
        Example_Check().Run(a => a with { Seed = 1833095082 });
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
        from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10), a => a.Trace())
        from spec in Checkr.Spec("List does not contain 7", () => !listInput.Contains(7))
        select Case.Closed;
    }
}