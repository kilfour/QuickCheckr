using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

namespace QuickCheckr.Tests.Concurrency;

public class AsyncLifetimeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task AllRunOverloadsRespectPreCancellation(int overload)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var token = cancellation.Token;
        var calls = 0;
        var check = Checkr.Act("must not run", () => { calls++; });
        var custodian = new InMemoryCustodian([]);
        var configured = check.Configure(c => c with { FileAs = "canceled", Custodian = custodian });
        Task unconfigured = overload switch
        {
            0 => check.RunAsync(token),
            1 => check.RunAsync(2.Runs(), token),
            2 => check.RunAsync(2.ExecutionsPerRun(), token),
            3 => check.RunAsync(2.Runs(), 2.ExecutionsPerRun(), token),
            4 => check.RunAsync(123, 2.ExecutionsPerRun(), token),
            _ => check.RunAsync(123, token)
        };
        Task withConfig = overload switch
        {
            0 => configured.RunAsync(token),
            1 => configured.RunAsync(2.Runs(), token),
            2 => configured.RunAsync(2.ExecutionsPerRun(), token),
            3 => configured.RunAsync(2.Runs(), 2.ExecutionsPerRun(), token),
            4 => configured.RunAsync(123, 2.ExecutionsPerRun(), token),
            _ => configured.RunAsync(123, token)
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => unconfigured);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => withConfig);
        Assert.True(unconfigured.IsCanceled);
        Assert.True(withConfig.IsCanceled);
        Assert.Equal(0, calls);
        Assert.Empty(custodian.Records);
        Assert.Empty(custodian.Entries);
    }

    [Theory]
    [InlineData("act")]
    [InlineData("careful")]
    [InlineData("careful when")]
    [InlineData("capture")]
    [InlineData("perform")]
    [InlineData("shrinkable")]
    [InlineData("shrinkable careful")]
    public async Task CanceledCallbacksAreNotCapturedAsFailures(string helper)
    {
        using var cancellation = new CancellationTokenSource();
        Func<Task<int>> canceled = async () =>
        {
            await Task.Yield();
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        };
        var step = helper switch
        {
            "act" => Checkr.Act("act", canceled).Select(_ => Case.Closed),
            "careful" => Checkr.ActCarefully("act", canceled).Select(_ => Case.Closed),
            "careful when" => Checkr.ActCarefullyWhen("act", () => true, canceled).Select(_ => Case.Closed),
            "capture" => Checkr.Capture(canceled).Select(_ => Case.Closed),
            "perform" => Checkr.Perform(async () => { await canceled(); }),
            "shrinkable" => Checkr.ShrinkableAct("act", canceled).Select(_ => Case.Closed),
            _ => Checkr.ShrinkableActCarefully("act", canceled).Select(_ => Case.Closed)
        };
        var custodian = new InMemoryCustodian([]);
        var pending = step.Configure(c => c with { FileAs = "canceled", Custodian = custodian }).RunAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Bounded(pending));

        Assert.True(pending.IsCanceled);
        Assert.Empty(custodian.Records);
        Assert.Empty(custodian.Entries);
    }

    [Fact]
    public async Task CancellationAwaitsCleanupAndPrefersAsyncDisposal()
    {
        using var cancellation = new CancellationTokenSource();
        var started = Gate();
        var disposalStarted = Gate();
        var releaseDisposal = Gate();
        var resource = new Resource(async () => { disposalStarted.TrySetResult(); await releaseDisposal.Task; });
        var continued = false;
        var custodian = new InMemoryCustodian([]);
        var check =
            from stash in Trackr.Stashed(() => resource)
            from work in Checkr.Act("wait", async token =>
            {
                Assert.Equal(cancellation.Token, token);
                started.TrySetResult();
                await Task.Delay(Timeout.Infinite, token);
            })
            from after in Checkr.Act("after", () => { continued = true; })
            select Case.Closed;
        var pending = check.Configure(c => c with { FileAs = "canceled", Custodian = custodian }).RunAsync(cancellation.Token);
        await Bounded(started.Task);
        cancellation.Cancel();
        try
        {
            await Bounded(disposalStarted.Task);
            Assert.False(pending.IsCompleted);
            Assert.False(continued);
        }
        finally { releaseDisposal.TrySetResult(); }
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Bounded(pending));

        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(1, resource.AsyncDisposals);
        Assert.Equal(0, resource.SyncDisposals);
        Assert.Empty(custodian.Records);
        Assert.Empty(custodian.Entries);
    }

    [Fact]
    public async Task CancellationDoesNotAbandonAnOperationThatIgnoresTheToken()
    {
        using var cancellation = new CancellationTokenSource();
        var release = Gate();
        var resource = new Resource(() => Task.CompletedTask);
        var check = from stash in Trackr.Stashed(() => resource)
                    from work in Checkr.Act("wait", async () => { await release.Task; })
                    select Case.Closed;
        var pending = check.RunAsync(cancellation.Token);
        cancellation.Cancel();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Equal(0, resource.AsyncDisposals);
        }
        finally { release.TrySetResult(); }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Bounded(pending));
        Assert.Equal(1, resource.AsyncDisposals);
    }

    [Fact]
    public async Task DisposalFailureStillDisposesTheRemainingStash()
    {
        var error = new InvalidOperationException("dispose failed");
        var broken = new Resource(async () => { await Task.Yield(); throw error; });
        var remaining = new Resource(async () => { await Task.Yield(); });
        var check = from first in Trackr.Stashed("first", () => broken)
                    from second in Trackr.Stashed("second", () => remaining)
                    select Case.Closed;

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => check.RunAsync(TestContext.Current.CancellationToken));

        Assert.Same(error, actual);
        Assert.Equal(1, broken.AsyncDisposals);
        Assert.Equal(1, remaining.AsyncDisposals);
        Assert.Equal(0, remaining.SyncDisposals);
    }

    private static TaskCompletionSource Gate() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    [Fact]
    public async Task CancellationDuringReplayDisposesResourcesWithoutFilingAFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var replayStarted = Gate();
        var resources = new List<Resource>();
        var calls = 0;
        var custodian = new InMemoryCustodian([]);
        var check =
            from resource in Trackr.Stashed(() =>
            {
                var value = new Resource(async () => { await Task.Yield(); });
                resources.Add(value);
                return value;
            })
            from action in Checkr.Act("work", async token =>
            {
                calls++;
                if (calls > 1)
                {
                    replayStarted.TrySetResult();
                    await Task.Delay(Timeout.Infinite, token);
                }
                return 42;
            })
            from expect in Checkr.Expect("fails", () => action < 0)
            select Case.Closed;
        var pending = check.Configure(c => c with { FileAs = "canceled replay", Custodian = custodian }).RunAsync(cancellation.Token);
        await Bounded(replayStarted.Task);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Bounded(pending));

        Assert.Equal(2, calls);
        Assert.All(resources, resource => Assert.Equal(1, resource.AsyncDisposals));
        Assert.Empty(custodian.Records);
        Assert.Empty(custodian.Entries);
    }

    private static Task Bounded(Task task) => task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

    private sealed class Resource(Func<Task> dispose) : IDisposable, IAsyncDisposable
    {
        public int AsyncDisposals { get; private set; }
        public int SyncDisposals { get; private set; }
        public void Dispose() => SyncDisposals++;
        public async ValueTask DisposeAsync() { AsyncDisposals++; await dispose(); }
    }
}
