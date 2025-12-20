using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;

[DocFile]
[DocFileHeader("ReportMode")]
[DocContent(
@"Controls which whether *Traces* and/or *Warnigns* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(ReportModeTests), nameof(GetCheckr))]
public class ReportModeTests : BaseTest
{
    public class Thing;

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
            from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
            from trace in Checkr.Trace("Irrelevant", () => irrelevant)
            from expect in Checkr.Expect("Counter does not equal 2", () => counter != 2)
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig ConfigureNone(CheckrConfig config)
        => config with { ReportMode = ReportMode.None };

    [Fact]
    [DocExample(typeof(ReportModeTests), nameof(ConfigureNone))]
    [DocCodeFile("ReportModeTestsNone.txt", "text")]
    public void RunTestNone()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun(), ConfigureNone));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsNone.txt", false);
        VerifyNone(article);
    }

    private static void VerifyNone(Article article)
    {
        Assert.Equal("Counter does not equal 2", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(0, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Counter does not equal 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    private static CheckrConfig ConfigureTrace(CheckrConfig config)
        => config with { ReportMode = ReportMode.Trace };

    [Fact]
    [DocExample(typeof(ReportModeTests), nameof(ConfigureTrace))]
    [DocCodeFile("ReportModeTestsTrace.txt", "text")]
    public void RunTestTrace()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun(), ConfigureTrace));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsTrace.txt", false);
        VerifyTrace(article);
    }

    private static void VerifyTrace(Article article)
    {
        Assert.Equal("Counter does not equal 2", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("73", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Counter does not equal 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    private static CheckrConfig ConfigureWarning(CheckrConfig config)
        => config with { ReportMode = ReportMode.Warning };

    [Fact]
    [DocContent("\n**`ReportMode.Inputs`**")]
    [DocExample(typeof(ReportModeTests), nameof(ConfigureWarning))]
    [DocCodeFile("ReportModeTestsWarning.txt", "text")]
    public void RunTestWarning()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun(), ConfigureWarning));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsWarning.txt", false);
        VerifyWarning(article);
    }

    private static void VerifyWarning(Article article)
    {
        Assert.Equal("Counter does not equal 2", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("Counter does not equal 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [Fact]
    [DocContent("The default settings includes all possible ReportModes.")]
    [DocCodeFile("ReportModeTestsDefault.txt", "text")]
    public void DefaultSetting()
    {
        Assert.True(ReportMode.Default.HasFlag(ReportMode.Trace));
        Assert.True(ReportMode.Default.HasFlag(ReportMode.Warning));

        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsDefault.txt", false);
        VerifyDefault(article);
    }

    private static void VerifyDefault(Article article)
    {
        Assert.Equal("Counter does not equal 2", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("73", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("Counter does not equal 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}