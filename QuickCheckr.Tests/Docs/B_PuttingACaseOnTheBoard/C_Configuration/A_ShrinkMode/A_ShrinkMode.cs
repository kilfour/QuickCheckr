using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.A_ShrinkMode;

[DocFile]
[DocFileHeader("ShrinkMode")]
[DocContent(
@"Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(A_ShrinkMode), nameof(GetCheckr))]
public class A_ShrinkMode : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from summer in Trackr.Stashed(() => new Summer())
            from input in Checkr.Input("Input", Fuzzr.Int(1, 25))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
            from add in Checkr.Act("Add", () => summer.Add(input))
            from expect in Checkr.Expect("Total should not be 42",
                () => summer.Total != 42)
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig ConfigureNone(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.None };

    [Fact]
    [DocContent("**`ShrinkMode.None`**")]
    [DocExample(typeof(A_ShrinkMode), nameof(ConfigureNone))]
    [DocContent("No shrinking. QuickCheckr reports the original failing run.")]
    [DocCodeFile("A_ShrinkModeNone.txt", "text")]
    public void RunTestNone()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1510302884, 5.ExecutionsPerRun(), ConfigureNone));
        WriteAsserts(article, false);
        WriteReport(article, "A_ShrinkModeNone.txt", false);
        VerifyNone(article);
    }

    private static void VerifyNone(Article article)
    {
        Assert.Equal("Total should not be 42", article.FailedExpectation());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(8, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal(11, article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Input(2).Read().Label);
        Assert.Equal(5, article.Execution(1).Input(2).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(2).Input(1).Read().Label);
        Assert.Equal(17, article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(2).Input(2).Read().Label);
        Assert.Equal(5, article.Execution(2).Input(2).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(3).Input(1).Read().Label);
        Assert.Equal(2, article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(3).Input(2).Read().Label);
        Assert.Equal(8, article.Execution(3).Input(2).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(4).Input(1).Read().Label);
        Assert.Equal(12, article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(4).Input(2).Read().Label);
        Assert.Equal(21, article.Execution(4).Input(2).Read().Value);
        Assert.Equal("Total should not be 42", article.PassedExpectation(1).Read().Label);
        Assert.Equal(3, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    private static CheckrConfig ConfigureExecutions(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Executions };

    [Fact]
    [DocContent("\n**`ShrinkMode.Executions`**")]
    [DocExample(typeof(A_ShrinkMode), nameof(ConfigureExecutions))]
    [DocContent("Trims away irrelevant executions.")]
    [DocCodeFile("A_ShrinkModeExecutions.txt", "text")]
    public void RunTestExecutions()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1510302884, 5.ExecutionsPerRun(), ConfigureExecutions));
        WriteAsserts(article, false);
        WriteReport(article, "A_ShrinkModeExecutions.txt", false);
        VerifyExecutions(article);
    }

    private static void VerifyExecutions(Article article)
    {
        Assert.Equal("Total should not be 42", article.FailedExpectation());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(8, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal(11, article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Input(2).Read().Label);
        Assert.Equal(5, article.Execution(1).Input(2).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(2).Input(1).Read().Label);
        Assert.Equal(17, article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(2).Input(2).Read().Label);
        Assert.Equal(5, article.Execution(2).Input(2).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(3).Input(1).Read().Label);
        Assert.Equal(2, article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(3).Input(2).Read().Label);
        Assert.Equal(8, article.Execution(3).Input(2).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(4).Input(1).Read().Label);
        Assert.Equal(12, article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(4).Input(2).Read().Label);
        Assert.Equal(21, article.Execution(4).Input(2).Read().Value);
        Assert.Equal("Total should not be 42", article.PassedExpectation(1).Read().Label);
        Assert.Equal(3, article.PassedExpectation(1).Read().TimesPassed);

    }

    [CodeExample]
    private static CheckrConfig ConfigureInputs(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Inputs };

    [Fact]
    [DocContent("\n**`ShrinkMode.Inputs`**")]
    [DocExample(typeof(A_ShrinkMode), nameof(ConfigureInputs))]
    [DocContent("Trims inputs to find a minimal failing case.")]
    [DocCodeFile("A_ShrinkModeInputs.txt", "text")]
    public void RunTestInputs()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1510302884, 5.ExecutionsPerRun(), ConfigureInputs));
        WriteAsserts(article, false);
        WriteReport(article, "A_ShrinkModeInputs.txt", false);
        VerifyInputs(article);
    }

    private static void VerifyInputs(Article article)
    {
        Assert.Equal("Total should not be 42", article.FailedExpectation());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(4, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("11", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("17", article.Execution(2).Input(1).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(3).Input(1).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("12", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Total should not be 42", article.PassedExpectation(1).Read().Label);
        Assert.Equal(3, article.PassedExpectation(1).Read().TimesPassed);

    }

    [CodeExample]
    private static CheckrConfig ConfigureReduction(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Reduction };

    [Fact]
    [DocContent("\n**`ShrinkMode.Reduction`**")]
    [DocExample(typeof(A_ShrinkMode), nameof(ConfigureReduction))]
    [DocContent("Applies configured reducers for fine-grained shrinking.")]
    [DocContent("**Note:** This is an experimental feature, and will be expanded in the near future. As of now reporting with only this mode active is not supported.")]
    //[DocCodeFile("A_ShrinkModeReduction.txt", "text")]
    public void RunTestReduction()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1609315627, 2.ExecutionsPerRun(), ConfigureReduction));
        WriteAsserts(article, false);
        WriteReport(article, "A_ShrinkModeReduction.txt", false);
        VerifyReduction(article);
    }

    private static void VerifyReduction(Article article)
    {

    }

    [Fact]
    [DocContent(
@"\n**`ShrinkMode.Default`**  

The default settings includes all possible ShrinkModes.")]
    public void DefaultSetting()
    {
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Executions));
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Inputs));
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Reduction));
    }

    public class Summer
    {
        public int Total { get; private set; } = 0;
        public void Add(int value) => Total += value;
    }
}