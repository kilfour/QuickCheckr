using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.A_TheNeedlerPattern;

[DocFile]
public class TheNeedlerPattern : BaseTest
{
    [Fact]
    [DocHeader("The SUT:")]
    [DocExample(typeof(Eventually))]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(TheNeedlerPattern), nameof(GetCheckr))]
    [DocHeader("The Report:")]
    [DocCodeFile("TheNeedlerPattern.txt", "text")]
    [DocHeader("The Needler:")]
    [DocExample(typeof(Needler<>))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(307579878, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<TheNeedlerPattern>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from needlers in Trackr.PoolFor<Needler<int>>()
            from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
            from needler in Checkr.Act("start", () => new Needler<int>().Start(ct => Eventually.AddOne(input)))
            from stored in Trackr.Pool("needler", needler)
            from no7 in Trackr.Analyze("Is not 7", () => needlers.All(a => a.Await() != 7))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Is not 7", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(9, article.ShrinkCount);
        Assert.Equal(5, article.Execution(1).Read().ExecutionId);
        Assert.Equal("start", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
    }

    [CodeExample]
    public class Eventually
    {
        public static async Task<int> AddOne(int input)
        {
            await Task.Delay(10);
            return input + 1;
        }
    }

    [CodeExample]
    public sealed class Needler<T>
    {
        public Task<T>? task;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public bool IsRunning => task is { IsCompleted: false };
        public Exception? Exception => task?.Exception?.InnerException;

        public Needler<T> Start(Func<CancellationToken, Task<T>> action)
        {
            if (IsRunning) return this;
            task = Task.Run(async () => await action(cancellationTokenSource.Token));
            return this;
        }

        public async Task StopAsync()
        {
            if (task == null) return;
            cancellationTokenSource.Cancel();
            try { await task; } finally { task = null; }
        }

        public T Await()
        {
            return task!.GetAwaiter().GetResult();
        }
    }
}


