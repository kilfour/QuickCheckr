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
            GetCheckr().Run(1878701932, 11.ExecutionsPerRun()));
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
            from ops in Checkr.OneOfIf(
                (() => needlers.Count() < 10,
                    from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
                    from needler in Checkr.Act("start", () => new Needler<int>().Start(ct => Eventually.AddOne(input)))
                    from stored in Trackr.Pool("needler", needler)
                    select Case.Closed),
                (() => needlers.Count() >= 10,
                    from results in Checkr.Act("wait", () => needlers.Select(a => a.Await()).ToArray())
                    from no42 in Checkr.Spec("Is not 7", () => results.All(a => a != 7))
                    select Case.Closed))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Is not 7", article.FailedSpec());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(9, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("start", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(11, article.Execution(2).Read().ExecutionId);
        Assert.Equal("wait", article.Execution(2).Action(1).Read().Label);
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
        private readonly CancellationTokenSource cancellationTokenSourcee = new();

        public bool IsRunning => task is { IsCompleted: false };
        public Exception? Exception => task?.Exception?.InnerException;

        public Needler<T> Start(Func<CancellationToken, Task<T>> action)
        {
            if (IsRunning) return this;
            task = Task.Run(async () => await action(cancellationTokenSourcee.Token));
            return this;
        }

        public async Task StopAsync()
        {
            if (task == null) return;
            cancellationTokenSourcee.Cancel();
            try { await task; } finally { task = null; }
        }

        public T Await()
        {
            return task!.GetAwaiter().GetResult();
        }
    }
}


