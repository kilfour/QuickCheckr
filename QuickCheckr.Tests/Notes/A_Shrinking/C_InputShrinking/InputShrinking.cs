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
            from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from spec in Checkr.Spec("Always Fails", () => false)
            select Case.Closed;
    }

    [Fact]
    [DocContent("Another example, slightly trickier.")]
    [DocExample(typeof(InputShrinking), nameof(Example_Two_Check))]
    [DocContent(
@"<Explanation goes here>

Result:
")]
    [DocCodeFile("InputShrinking_Two.txt", "text")]
    public void Example_Two()
    {
        var article = TheJournalist.Exposes(Example_Two_Test);
        Assert.Equal("Should not be 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs()); // should be zero
        var execution = article.Execution(1).Read();
        Assert.Equal(1, execution.ExecutionId);


        var report = TheClerk.Transcribes(article.CaseFile);
        var reader = LinesReader.FromText(report);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Test:                    Example_Two_Test", reader.NextLine());
        Assert.Contains(" Location: ", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Seed:                    231122398", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (1): Result", reader.NextLine());
        Assert.Equal("   - Input: Input One = 12", reader.NextLine());
        Assert.Equal("   - Input: Input Three = 30", reader.NextLine());
        Assert.Equal(" ====================================", reader.NextLine());
        Assert.Equal("  !! Spec Failed: Should not be 42", reader.NextLine());
        Assert.Equal(" ====================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
        if (WriteReportToDisk)
            WriteReport.ToFile("InputShrinking_Two.txt", report);
    }

    private static void Example_Two_Test()
    {
        Example_Two_Check().Run(
            1.Runs(),
            1.ExecutionsPerRun(),
            a => a with { Seed = 231122398 });
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Two_Check()
    {
        return
            from one in Checkr.Input("Input One", Fuzzr.Int())
            from two in Checkr.Input("Input Two", Fuzzr.Int())
            from three in Checkr.Input("Input Three", Fuzzr.Int())
            from result in Checkr.Act("Result", () => one + three)
            from spec in Checkr.Spec("Should not be 42", () => result != 42)
            select Case.Closed;
    }
}
