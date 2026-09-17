using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification;

public class AsyncExpectationTests
{
    [Fact]
    public async Task AwaitedFailureUsesMessagesOnlyAfterCompletion()
    {
        var state = State(TestContext.Current.CancellationToken);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var messages = 0;
        var pending = Checkr.Expect("check", () => gate.Task, () => { messages++; return "failed"; })(state).AsTask();
        Assert.False(pending.IsCompleted);
        Assert.Equal(0, messages);
        gate.SetResult(false);
        await pending;
        Assert.True(state.EvidenceRecorder.Failed());
        Assert.Equal(["failed"], state.EvidenceRecorder.FailingExpectation.Message);
        Assert.Equal(1, messages);
    }

    [Fact]
    public async Task SkippedReplayDoesNotInvokeAsyncCallback()
    {
        var state = State(TestContext.Current.CancellationToken);
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("target", []), 1);
        state.StartReplay();
        var invoked = false;
        await Checkr.Expect("other", async () => { invoked = true; await Task.Yield(); return false; })(state);
        Assert.False(invoked);
    }

    [Fact]
    public async Task ExceptionAfterSuspensionUsesExpectationCrashHandling()
    {
        var state = State(TestContext.Current.CancellationToken);
        var check = Checkr.Expect("check", async () => { await Task.Yield(); throw new InvalidOperationException("after await"); });
        var error = await Assert.ThrowsAsync<ExpectationThrowsException>(() => check(state).AsTask());
        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReplayAwaitsFailureAndRejectsExceptionAfterSuspension(bool testimony)
    {
        var state = State(TestContext.Current.CancellationToken);
        state.Memory.StoreInput("number", () => 1);
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("check", []), 1);
        var check = Checkr.Expect("check", async () =>
        {
            await Task.Yield();
            if ((int)state.Memory.GetCurrentInputValue(1, "number")! < 0)
                throw new InvalidOperationException("invalid candidate");
            return false;
        });
        var verifier = new Verifier(state, check);
        var rejected = testimony ? await verifier.TestimonyAsync(1, "number", -1) : await verifier.RunAsync(1, "number", -1);
        Assert.IsType<ReplayOutcome.CandidateRejected>(rejected);
        var preserved = testimony ? await verifier.TestimonyAsync(1, "number", 2) : await verifier.RunAsync(1, "number", 2);
        Assert.IsType<ReplayOutcome.FailurePreserved>(preserved);
    }

    [Fact]
    public async Task CancellationAfterAwaitDoesNotRecordFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var state = State(cancellation.Token);
        var check = Checkr.Expect("check", async token =>
        {
            Assert.Equal(cancellation.Token, token);
            await Task.Yield();
            cancellation.Cancel();
            return false;
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => check(state).AsTask());
        Assert.False(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task RelatedPoolsAreAwaitedSequentiallyIncludingEmptyChildCollections()
    {
        var state = State(TestContext.Current.CancellationToken);
        state.Memory.StoreInPool(1, "first");
        state.Memory.StoreInPool(2, "empty");
        state.Memory.StoreInPool(1, 7);
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var check = Trackr.PoolExpectEach<string, int>("related", (parent, _) => parent == "first",
            async (parent, children) =>
            {
                calls.Add(parent);
                if (parent == "first")
                {
                    Assert.Equal([7], children);
                    await gate.Task;
                }
                else Assert.Empty(children);
                return true;
            });
        var pending = check(state).AsTask();
        Assert.False(pending.IsCompleted);
        Assert.Equal(["first"], calls);
        gate.SetResult(true);
        await pending;
        Assert.Equal(["first", "empty"], calls);
        Assert.Equal(2, state.EvidenceRecorder.PassedExpectations.Count);
    }

    [Fact]
    public async Task RelatedPoolReplayInvokesOnlyTargetedParent()
    {
        var state = State(TestContext.Current.CancellationToken);
        state.Memory.StoreInPool(1, "first");
        state.Memory.StoreInPool(2, "second");
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("related: String-2", []), 1);
        state.StartReplay();
        // Pools are rebuilt during replay.
        state.Memory.StoreInPool(1, "first");
        state.Memory.StoreInPool(2, "second");
        var calls = new List<string>();
        await Trackr.PoolExpectEach<string, int>("related", (_, _) => true, async (parent, children) =>
        {
            await Task.Yield();
            calls.Add(parent);
            return false;
        })(state);
        Assert.Equal(["second"], calls);
        Assert.True(state.EvidenceRecorder.Failed());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EachPoolVariantReportsFailedElementsOnce(bool filtered)
    {
        var state = State(TestContext.Current.CancellationToken);
        state.Memory.StoreInPool(1, 1);
        state.Memory.StoreInPool(2, 2);
        var calls = new List<int>();
        async Task<bool> Condition(int item) { await Task.Yield(); calls.Add(item); return false; }
        var check = filtered
            ? Trackr.PoolExpectEachWhen<int>("each", item => item == 2, Condition)
            : Trackr.PoolExpectEach<int>("each", Condition);
        await check(state);
        Assert.Equal(filtered ? [2] : new[] { 1, 2 }, calls);
        Assert.Equal(filtered ? ["Int32-2"] : new[] { "Int32-1", "Int32-2" }, state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task WholePoolCallbackIsAwaited()
    {
        var state = State(TestContext.Current.CancellationToken);
        state.Memory.StoreInPool(1, 42);
        await Trackr.PoolExpect<int>("pool", async items => { await Task.Yield(); return items.Single() == 42; })(state);
        Assert.Equal(1, state.EvidenceRecorder.PassedExpectations["pool"]);
    }

    [Fact]
    public async Task FullRunnerShrinksWithSuspendedRelatedPoolExpectation()
    {
        var calls = 0;
        var check =
            from input in Checkr.Input("irrelevant", Fuzzr.Constant(42))
            from parent in Trackr.ToPool("parent", () => "board")
            from expectation in Trackr.PoolExpectEach<string, int>("related", (_, _) => true,
                async (_, _) => { await Task.Yield(); calls++; return false; })
            select Case.Closed;
        var file = await new CheckrRunner(check, new CheckrRunnerConfig(CheckrConfig.Default()), 1, AutopsyProbe.Empty).RunAsync();
        Assert.True(file.HasEvidence);
        Assert.True(file.ShrinkCount > 0);
        Assert.Equal(0, file.InputCount);
        Assert.True(calls > 1);
    }

    [Fact]
    public async Task PreCancelledRunNeverInvokesCallback()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var state = State(cancellation.Token);
        var called = false;
        var check = Checkr.Expect("cancelled", async () => { called = true; await Task.Yield(); return true; });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => check(state).AsTask());
        Assert.False(called);
    }

    [Fact]
    public async Task EmptyPoolsDoNotInvokeEachCallbacks()
    {
        var state = State(TestContext.Current.CancellationToken);
        var called = false;
        async Task<bool> Condition(int _) { called = true; await Task.Yield(); return false; }
        await Trackr.PoolExpectEach<int>("empty", Condition)(state);
        await Trackr.PoolExpectEachWhen<int>("filtered empty", _ => true, Condition)(state);
        await Trackr.PoolExpectEach<string, int>("related empty", (_, _) => true,
            async (_, _) => { called = true; await Task.Yield(); return false; })(state);
        Assert.False(called);
        Assert.False(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task CancellationStopsPoolBeforeNextElement()
    {
        using var cancellation = new CancellationTokenSource();
        var state = State(cancellation.Token);
        state.Memory.StoreInPool(1, 1);
        state.Memory.StoreInPool(2, 2);
        var calls = 0;
        var check = Trackr.PoolExpectEach<int>("cancelled", async (item, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            calls++;
            await Task.Yield();
            cancellation.Cancel();
            return false;
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => check(state).AsTask());
        Assert.Equal(1, calls);
        Assert.False(state.EvidenceRecorder.Failed());
    }
    private static CheckrState State(CancellationToken token)
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty, token);
        state.Memory.StartNewExecution();
        return state;
    }
}
