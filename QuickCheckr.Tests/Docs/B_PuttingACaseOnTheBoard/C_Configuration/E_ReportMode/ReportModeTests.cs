using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;


[DocFileHeader("ReportMode")]
[DocContent(
@"Controls whether or not *Traces*, *Warnings*  and/or *Exception StackTraces* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(ReportModeTests), nameof(GetCheckr))]
[DocInclude(typeof(ReportModeNone))]
public class ReportModeTests : BaseTest
{
    public class Thing;

    [CodeSnippet]
    [CodeRemove("return")]
    public static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
            from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
            from trace in Checkr.Trace("Irrelevant", () => irrelevant)
            from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig ConfigureTrace(CheckrConfig config)
        => config with { ReportMode = ReportMode.Trace };

    [Fact]
    [DocHeader("ReportMode.Trace", 1)]
    [DocContent("Only traces.")]
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
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Equal("ComputerSaysNo: Only one is available", article.GetExceptionMessage());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("73", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeExample]
    private static CheckrConfig ConfigureWarning(CheckrConfig config)
        => config with { ReportMode = ReportMode.Warning };

    [Fact]
    [DocHeader("ReportMode.Warning", 1)]
    [DocContent("Only warnings.")]
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
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Equal("ComputerSaysNo: Only one is available", article.GetExceptionMessage());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
    }

    [CodeExample]
    private static CheckrConfig ConfigureStackTrace(CheckrConfig config)
        => config with { ReportMode = ReportMode.StackTrace };

    [Fact]
    [DocHeader("ReportMode.StackTrace", 1)]
    [DocContent("Includes the StackTrace in the failure description if an exception is thrown.")]
    [DocExample(typeof(ReportModeTests), nameof(ConfigureStackTrace))]
    [DocCodeFile("ReportModeTestsStackTrace.txt", "text")]
    public void RunTestStackTrace()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun(), ConfigureStackTrace));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsStackTrace.txt", false);
        VerifyStackTrace(article);
    }

    private static void VerifyStackTrace(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Contains("at QuickPulse.Instruments.ComputerSays.No", article.GetExceptionMessage());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
    }

    [Fact]
    [DocHeader("ReportMode.Default", 1)]
    [DocContent("Includes all possible ReportModes.")]
    [DocCodeFile("ReportModeTestsDefault.txt", "text")]
    public void DefaultSetting()
    {
        Assert.True(ReportMode.Default.HasFlag(ReportMode.Trace));
        Assert.True(ReportMode.Default.HasFlag(ReportMode.Warning));
        Assert.True(ReportMode.Default.HasFlag(ReportMode.StackTrace));

        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1964562926, 2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "ReportModeTestsDefault.txt", false);
        VerifyDefault(article);
    }

    private static void VerifyDefault(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Contains("at QuickPulse.Instruments.ComputerSays.No", article.GetExceptionMessage());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("73", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
    }
}