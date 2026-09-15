using System.Collections.Concurrent;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Concurrency;

public class SynchronizationContextTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsyncCompletesOnSingleThreadedContext(bool fail)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            using var context = new SingleThreadedContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                // Invoke before pumping: a blocking wait here prevents Task.Yield from resuming.
                var pending = ExerciseAsync(context, fail, cancellationToken);
                _ = pending.ContinueWith(_ => context.Complete(), CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                context.Pump();
                pending.GetAwaiter().GetResult(); // Already completed; propagate test failures.
                completion.TrySetResult();
            }
            catch (Exception error) { completion.TrySetException(error); }
            finally { SynchronizationContext.SetSynchronizationContext(null); }
        }) { IsBackground = true, Name = "QuickCheckr synchronization context regression" };

        // A regression can block the dedicated thread, but cannot strand the test runner.
        thread.Start();
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
    }

    private static async Task ExerciseAsync(SynchronizationContext context, bool fail, CancellationToken cancellationToken)
    {
        var calls = 0;
        var disposals = 0;
        var check =
            from resource in Trackr.Stashed(() => new AsyncResource(() => disposals++))
            from action in Checkr.Act("yield", async () =>
            {
                var captured = SynchronizationContext.Current;
                var threadId = Environment.CurrentManagedThreadId;
                if (calls++ == 0) Assert.Same(context, captured);
                await Task.Yield();
                if (captured != null)
                {
                    Assert.Same(captured, SynchronizationContext.Current);
                    Assert.Equal(threadId, Environment.CurrentManagedThreadId);
                }
            })
            from expect in Checkr.Expect("deliberate failure", () => !fail)
            select Case.Closed;

        if (fail)
        {
            var error = await Assert.ThrowsAsync<FalsifiableException>(() => check.RunAsync(cancellationToken));
            Assert.Contains("deliberate failure", error.Message);
            Assert.True(calls > 1); // Failure replay also awaits the callback and disposal.
        }
        else
        {
            await check.RunAsync(cancellationToken);
            Assert.Equal(1, calls);
        }
        Assert.Equal(calls, disposals);
        Assert.Same(context, SynchronizationContext.Current);
    }

    private sealed class AsyncResource(Action disposed) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Task.Yield();
            disposed();
        }
    }

    private sealed class SingleThreadedContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> queue = new();

        public override void Post(SendOrPostCallback callback, object? state) => queue.Add((callback, state));
        public override void Send(SendOrPostCallback callback, object? state) =>
            throw new NotSupportedException("Synchronous dispatch is deliberately unsupported.");
        public override SynchronizationContext CreateCopy() => this;
        public void Complete() => queue.CompleteAdding();
        public void Pump()
        {
            foreach (var item in queue.GetConsumingEnumerable()) item.Callback(item.State);
        }
        public void Dispose() => queue.Dispose();
    }
}
