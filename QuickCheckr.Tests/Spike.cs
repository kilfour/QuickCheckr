using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;

namespace QuickCheckr.Tests;

public class Spike
{
    [Fact(Skip = "just wondering")]
    public void LetsSee()
    {
        var checkr =
            from needlers in Trackr.PoolFor<TaskNeedler<int>>()
            from ops in Checkr.OneOfIf(
                (() => needlers.Count() < 10,
                    from input in Checkr.Input("Input", Fuzzr.Int())
                    from needler in Checkr.Act("start", () => new TaskNeedler<int>().Start(ct => Eventually.AddOne(input)))
                    from stored in Trackr.Pool("needler", needler)
                    select Case.Closed),
                (() => needlers.Count() >= 10,
                    from results in Checkr.Act("wait", () => needlers.Select(a => a.Await()).ToArray())
                    from no42 in Checkr.Spec("Is not 42", () => results.All(a => a != 42))
                    select Case.Closed))
            select Case.Closed;
        TheJournalist.Exposes(() => checkr.Run(200.Runs(), 11.ExecutionsPerRun(), a => a with { FileAs = "Spike" }));
    }

    public class Eventually
    {
        public static async Task<int> AddOne(int input)
        {
            await Task.Delay(10);
            return input + 1;
        }
    }
}

public sealed class TaskNeedler<T>
{
    public Task<T>? task;
    private readonly CancellationTokenSource cancellationTokenSourcee = new();

    public bool IsRunning => task is { IsCompleted: false };
    public Exception? Exception => task?.Exception?.InnerException;

    public TaskNeedler<T> Start(Func<CancellationToken, Task<T>> action)
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
