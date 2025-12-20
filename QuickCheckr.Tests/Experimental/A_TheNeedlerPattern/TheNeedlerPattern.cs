using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.A_TheNeedlerPattern;

[DocFile]
public class TheNeedlerPattern : BaseTest
{
    [CodeExample]
    public class Eventually
    {
        public static async Task<int> AddOne(int input)
        {
            await Task.Delay(10);
            return input + 1;
        }
    }

    [Fact]
    [DocHeader("The SUT:")]
    [DocExample(typeof(Eventually))]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(TheNeedlerPattern), nameof(GetCheckrBuiltIn))]
    [DocHeader("The Report:")]
    [DocCodeFile("TheNeedlerPattern.txt", "text")]
    [DocHeader("Where to go from here:")]
    [DocRawFile("taskr-todo.md")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrBuiltIn().Run(307579878, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<TheNeedlerPattern>(article, false);
        VerifyBuiltIn(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrBuiltIn()
    {
        return
            from tasks in Taskr.PoolFor<int>()
            from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
            from task in Taskr.Start("start", ct => Eventually.AddOne(input))
            from no7 in Trackr.Expect("Is not 7", () => tasks.All(a => a.GetAwaiter().GetResult() != 7))
            select Case.Closed;
    }

    private static void VerifyBuiltIn(Article article)
    {
        Assert.Equal("Is not 7", article.FailedExpectation());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(9, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("start", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(10, article.Execution(2).Read().ExecutionId);
        Assert.Equal("start", article.Execution(2).Action(1).Read().Label);
    }
}