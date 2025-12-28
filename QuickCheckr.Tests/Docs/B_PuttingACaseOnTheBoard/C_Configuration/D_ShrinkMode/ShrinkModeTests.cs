using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.D_ShrinkMode;

//[DocFile]
[DocFileHeader("ShrinkMode")]
[DocContent(
@"Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(ShrinkModeTests), nameof(GetCheckr))]
public class ShrinkModeTests : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
            from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig ConfigureNone(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.None };


    [Fact]
    [DocExample(typeof(ShrinkModeTests), nameof(ConfigureNone))]
    [DocHeader("ShrinkMode.None", 1)]
    [DocContent("QuickCheckr reports the original failing run.")]
    [DocCodeFile("ShrinkModeTestsNone.txt", "text")]
    public void RunTestNone()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1044984793, 3.ExecutionsPerRun(), ConfigureNone));
        WriteAsserts(article, false);
        WriteReport(article, "ShrinkModeTestsNone.txt", false);
        VerifyNone(article);
    }

    private static void VerifyNone(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailureDescription());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(6, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal(1, article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Input(2).Read().Label);
        Assert.Equal(13, article.Execution(1).Input(2).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal(2, article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(2).Input(2).Read().Label);
        Assert.Equal(5, article.Execution(2).Input(2).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(3).Input(1).Read().Label);
        Assert.Equal(3, article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(3).Input(2).Read().Label);
        Assert.Equal(8, article.Execution(3).Input(2).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);

    }

    [CodeExample]
    private static CheckrConfig ConfigureExecutions(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Executions };

    [Fact]
    [DocHeader("ShrinkMode.Executions", 1)]
    [DocContent("Trims away irrelevant executions.")]
    [DocExample(typeof(ShrinkModeTests), nameof(ConfigureExecutions))]
    [DocCodeFile("ShrinkModeTestsExecutions.txt", "text")]
    public void RunTestExecutions()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1044984793, 3.ExecutionsPerRun(), ConfigureExecutions));
        WriteAsserts(article, false);
        WriteReport(article, "ShrinkModeTestsExecutions.txt", false);
        VerifyExecutions(article);
    }

    private static void VerifyExecutions(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal(3, article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Input(2).Read().Label);
        Assert.Equal(8, article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);

    }

    [CodeExample]
    private static CheckrConfig ConfigureInputs(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Inputs };

    [Fact]
    [DocHeader("ShrinkMode.Inputs", 1)]
    [DocContent("Trims inputs to find a minimal failing case.")]
    [DocExample(typeof(ShrinkModeTests), nameof(ConfigureInputs))]
    [DocCodeFile("ShrinkModeTestsInputs.txt", "text")]
    public void RunTestInputs()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1044984793, 3.ExecutionsPerRun(), ConfigureInputs));
        WriteAsserts(article, false);
        WriteReport(article, "ShrinkModeTestsInputs.txt", false);
        VerifyInputs(article);
    }

    private static void VerifyInputs(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Read().Times);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    private static CheckrConfig ConfigureReduction(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Reduction };

    [Fact]
    [DocHeader("ShrinkMode.Reduction", 1)]
    [DocContent("Applies configured reducers for fine-grained shrinking.")]
    [DocExample(typeof(ShrinkModeTests), nameof(ConfigureReduction))]
    [DocExperimental]
    [DocContent("*Note:* As of now reporting with only this mode active is not fully supported.")]
    //[DocCodeFile("ShrinkModeTestsReduction.txt", "text")]
    public void RunTestReduction()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1044984793, 3.ExecutionsPerRun(), ConfigureReduction));
        WriteAsserts(article, false);
        WriteReport(article, "ShrinkModeTestsReduction.txt", false);
        VerifyReduction(article);
    }

    private static void VerifyReduction(Article article)
    {

    }

    [Fact]
    [DocHeader("ShrinkMode.Default", 1)]
    [DocContent("Includes all possible ShrinkModes.")]
    [DocCodeFile("ShrinkModeTestsDefault.txt", "text")]
    public void DefaultSetting()
    {
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Executions));
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Inputs));
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Reduction));

        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1044984793, 3.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "ShrinkModeTestsDefault.txt", false);
        VerifyDefault(article);
    }

    private static void VerifyDefault(Article article)
    {
        Assert.Equal("Counter does not equal 3", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Counter does not equal 3", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}