using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ActionShrinking;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Runners.Tracing;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Concurrency;

public class AwaitableShrinkingTests
{
    [Fact]
    public async Task ReductionWaitsForVerificationBeforeGeneratingTheNextCandidate()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var observations = new List<ReductionResult>();
        var candidates = new List<int>();
        Reduction Next(ReductionResult result)
        {
            observations.Add(result);
            return observations.Count switch
            {
                1 => new Reduction.Next(9),
                2 => new Reduction.Next(8),
                _ => new Reduction.Stop(result.Value!)
            };
        }
        async ValueTask<bool> Verify(object candidate)
        {
            candidates.Add((int)candidate);
            await release.Task;
            await Task.Yield();
            return (int)candidate == 8;
        }

        var pending = ReductionRunner.ForReducerAsync(Next, "input", "input", 10, Verify, new Whistleblower()).AsTask();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Single(observations);
            Assert.Equal(new[] { 9 }, candidates);
        }
        finally { release.TrySetResult(); }
        var result = Assert.IsType<Reduction.Stop>(await pending.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        Assert.Equal(8, result.Result);
        Assert.Equal(new[] { 9, 8 }, candidates);
        Assert.Equal(new[] { new ReductionResult(true, 10), new ReductionResult(false, 10), new ReductionResult(true, 8) }, observations);
    }

    [Fact]
    public async Task ReductionPropagatesAFaultAfterSuspensionWithoutTryingAnotherCandidate()
    {
        var steps = 0;
        var expected = new InvalidOperationException("verification failed");
        Reduction Next(ReductionResult result) { steps++; return new Reduction.Next(1); }
        async ValueTask<bool> Verify(object candidate) { await Task.Yield(); throw expected; }

        var pending = ReductionRunner.ForReducerAsync(Next, "input", "input", 10, Verify, new Whistleblower()).AsTask();
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => pending.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        Assert.Same(expected, actual);
        Assert.Equal(1, steps);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DispatchAwaitsHandlersInOrderAndStopsAtTheFirstMatch(bool collection)
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = new List<string>();
        FirstMatchHandler<int, List<string>, string>[] handlers =
        [
            new Handler(async (input, log) =>
            {
                log.Add("first");
                await release.Task;
                return HandlerResult.Unhandled<string>();
            }),
            new Handler((input, log) =>
            {
                log.Add("second");
                return new(HandlerResult.Handled("selected"));
            }),
            new Handler((input, log) => throw new InvalidOperationException("third handler must not run"))
        ];
        var pending = collection
            ? new FirstMatchCollectionDispatcher<int, List<string>, string>(handlers, new CollectionFallback()).DispatchAsync([1, 2], calls).AsTask()
            : new FirstMatchDispatcher<int, List<string>, string>(handlers, new Fallback()).DispatchAsync(1, calls).AsTask();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Equal(new[] { "first" }, calls);
        }
        finally { release.TrySetResult(); }

        Assert.Equal("selected", await pending.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
        Assert.Equal(new[] { "first", "second" }, calls);
    }

    [Theory]
    [InlineData("initial")]
    [InlineData("trace")]
    [InlineData("final trace")]
    [InlineData("case selection")]
    [InlineData("execution shrinking")]
    [InlineData("action shrinking")]
    [InlineData("verification")]
    [InlineData("testimony")]
    public async Task ReplayFailuresDisposeTheStashAndRestoreTemporaryState(string phase)
    {
        var state = new CheckrState(2, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("input", () => 42);
        state.Memory.StoreShrinkableAction("action");
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("input", () => 42);
        var disposable = new DisposalSpy();
        var expected = new InvalidOperationException("script failed");
        CheckrOf<Case> check = async s =>
        {
            s.Memory.StoreStashed("resource", () => disposable);
            if (phase is "verification" or "testimony")
                Assert.Equal(7, s.Memory.GetCurrentInputValue(1, "input"));
            await Task.Yield();
            throw expected;
        };

        Task pending = phase switch
        {
            "initial" => new InitialRun(state, check).RunAsync(1).AsTask(),
            "trace" => new TraceRun(state, check).RunAsync(true).AsTask(),
            "final trace" => new FinalTraceRun(state, check).RunAsync().AsTask(),
            "case selection" => new CaseSelectionRun(state, check, true, true, true).RunAsync().AsTask(),
            "execution shrinking" => new ExecutionShrinker(state, check).RunAsync().AsTask(),
            "action shrinking" => new ActionShrinker(state, check).RunAsync().AsTask(),
            "verification" => new Verifier(state, check).RunAsync(1, "input", 7).AsTask(),
            _ => new Verifier(state, check).TestimonyAsync(1, "input", 7).AsTask()
        };
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => pending.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        Assert.Same(expected, actual);
        Assert.Equal(1, disposable.Count);
        Assert.False(state.Memory.Tracing);
        Assert.False(state.Memory.FinalTrace);
        Assert.False(state.Memory.IsConfigRun);
        Assert.False(state.Memory.Deliberating);
        Assert.False(state.Memory.Distinguishing);
        Assert.False(state.Memory.RejectingCases);
        Assert.Equal(42, state.Memory.GetCurrentInputValue(1, "input"));
        if (phase == "execution shrinking")
            Assert.Equal(new[] { 1, 2 }, state.Memory.Numbers);
        if (phase == "action shrinking")
            Assert.Contains("action", state.Memory.Entries().First().Item2.GetActions());
    }

    private sealed class Handler(Func<int, List<string>, ValueTask<HandlerResult<string>>> handle)
        : FirstMatchHandler<int, List<string>, string>
    {
        public override ValueTask<HandlerResult<string>> TryHandleAsync(int input, List<string> context) => handle(input, context);
    }

    private sealed class Fallback : FirstMatchFallback<int, List<string>, string>
    {
        public override string Handle(int input, List<string> context) => throw new InvalidOperationException("fallback must not run");
    }

    private sealed class CollectionFallback : FirstMatchCollectionFallback<List<string>, string>
    {
        public override string Handle(List<string> context) => throw new InvalidOperationException("fallback must not run");
    }

    private sealed class Whistleblower : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 100;
        public void Warn(string key, Func<Warning> factory) => throw new InvalidOperationException("unexpected warning");
    }

    private sealed class DisposalSpy : IDisposable
    {
        public int Count { get; private set; }
        public void Dispose() => Count++;
    }
}
