using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickFuzzr;

namespace QuickCheckr.Tests.Concurrency;

public class AsyncCompositionTests
{
    [Fact]
    public async Task SynchronousStepsReturnCompletedResults()
    {
        var state = CreateState();
        var expected = CheckrResult.Some(state, 42);
        CheckrOf<int> step = _ => expected;
        var pending = step(state);

        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Same(expected, await pending);
    }

    [Fact]
    public async Task LinqWaitsForEachStepBeforeSelectingAndProjecting()
    {
        var release = Gate();
        var calls = new List<string>();
        CheckrOf<int> source = async state =>
        {
            calls.Add("source started");
            await release.Task;
            calls.Add("source completed");
            return CheckrResult.Some(state, 20);
        };
        CheckrOf<int> Next(int value) => async state =>
        {
            calls.Add("next started");
            await Task.Yield();
            calls.Add("next completed");
            return CheckrResult.Some(state, value + 2);
        };
        var check = from left in source from right in Next(left) select left + right;
        var pending = check(CreateState()).AsTask();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Equal(new[] { "source started" }, calls);
        }
        finally { release.TrySetResult(); }

        Assert.Equal(42, (await Bounded(pending)).Value);
        Assert.Equal(new[] { "source started", "source completed", "next started", "next completed" }, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StopAfterSuspensionSkipsSelectorsAndProjectors(bool bind)
    {
        var selected = false;
        CheckrOf<int> source = async state =>
        {
            await Task.Yield();
            state.StopRun();
            return CheckrResult.Some(state, 42);
        };
        var check = bind
            ? source.SelectMany(value => { selected = true; return Checkr.Input("next", Fuzzr.Constant(value)); })
            : source.Select(value => { selected = true; return value; });

        await Bounded(check(CreateState()).AsTask());

        Assert.False(selected);
    }

    [Theory]
    [InlineData("when")]
    [InlineData("one of")]
    [InlineData("one of when")]
    [InlineData("sequence")]
    [InlineData("once")]
    [InlineData("two pools")]
    [InlineData("pool when")]
    [InlineData("two pools when")]
    [InlineData("for each")]
    [InlineData("for each when")]
    public async Task CompositionWaitsForANestedScript(string operation)
    {
        var release = Gate();
        var completed = 0;
        var continued = false;
        CheckrOf<Case> nested = async state =>
        {
            await release.Task;
            completed++;
            return CheckrResult.CaseOnly(state);
        };
        CheckrOf<CheckrOption> option = async state =>
        {
            await Task.Yield();
            return CheckrResult.Some<CheckrOption>(state, new(() => true, nested));
        };
        var wrapper = operation switch
        {
            "when" => Checkr.When(() => true, nested).Select(_ => Case.Closed),
            "one of" => Checkr.OneOf(nested),
            "one of when" => Checkr.OneOfWhen(option),
            "sequence" => Checkr.Sequence(nested),
            "once" => Trackr.Once("once", nested),
            "two pools" => Trackr.OneOfPool<int, string>("pools", (_, _) => nested),
            "pool when" => Trackr.OneOfPoolWhen<int>("pool", _ => true, _ => nested),
            "two pools when" => Trackr.OneOfPoolWhen<int, string>("pools", (_, _) => true, (_, _) => nested),
            "for each" => Trackr.PoolForEach<int>(_ => nested),
            _ => Trackr.PoolForEachWhen<int>(_ => true, _ => nested)
        };
        CheckrOf<Case> seedPools = state =>
        {
            state.Memory.StoreInPool(1, 10);
            state.Memory.StoreInPool(2, "item");
            return CheckrResult.CaseOnly(state);
        };
        var check = from seed in seedPools
                    from wrapped in wrapper
                    from after in Checkr.Act("after", () => { continued = true; })
                    select Case.Closed;

        var pending = check.RunAsync(123, TestContext.Current.CancellationToken);
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.False(continued);
            Assert.Equal(0, completed);
        }
        finally { release.TrySetResult(); }
        await Bounded(pending);

        Assert.True(continued);
        Assert.Equal(1, completed);
    }

    [Fact]
    public async Task FalseConditionDoesNotInvokeTheAsyncScript()
    {
        var calls = 0;
        CheckrOf<Case> nested = async state =>
        {
            calls++;
            await Task.Yield();
            return CheckrResult.CaseOnly(state);
        };

        await Bounded(Checkr.When(() => false, nested).Select(_ => Case.Closed).RunAsync(TestContext.Current.CancellationToken));

        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerifierKeepsTheCandidateInScopeAcrossSuspension(bool testimony)
    {
        var release = Gate();
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("input", () => 42);
        await Checkr.Expect("fails", () => false)(state);
        CheckrOf<Case> replay = async s =>
        {
            Assert.Equal(7, s.Memory.GetCurrentInputValue(1, "input"));
            await release.Task;
            Assert.Equal(7, s.Memory.GetCurrentInputValue(1, "input"));
            return await Checkr.Expect("fails", () => false)(s);
        };
        var verifier = new Verifier(state, replay);
        var pending = (testimony ? verifier.TestimonyAsync(1, "input", 7) : verifier.RunAsync(1, "input", 7)).AsTask();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Equal(7, state.Memory.GetCurrentInputValue(1, "input"));
        }
        finally { release.TrySetResult(); }

        Assert.IsType<ReplayOutcome.FailurePreserved>(await Bounded(pending));
        Assert.Equal(42, state.Memory.GetCurrentInputValue(1, "input"));
    }

    [Fact]
    public async Task SuspendingScriptsPreserveShrinkingAndReporting()
    {
        var phases = new HashSet<string>();
        CheckrOf<Case> Pause(bool suspend) => async state =>
        {
            if (suspend) await Task.Yield();
            phases.Add(state.IsInitialRun ? "initial" : "replay");
            if (state.Memory.Tracing) phases.Add("trace");
            if (state.Memory.FinalTrace) phases.Add("final trace");
            if (state.Memory.Distinguishing) phases.Add("selection");
            return CheckrResult.CaseOnly(state);
        };
        ConfiguredCheckr Create(bool suspend) =>
            (from count in Trackr.Stashed(() => new Counter())
             from input in Checkr.Input("input", Fuzzr.Constant(42))
             from increment in Checkr.ShrinkableAct("increment", () => { count.Value++; })
             from irrelevant in Checkr.ShrinkableAct("irrelevant", () => { })
             from pause in Pause(suspend)
             from distinction in Checkr.Distinguish(() => "category")
             from trace in Checkr.Trace("input", a => a.Show(input))
             from expect in Checkr.Expect("below threshold", () => count.Value < 2 || input < 10)
             select Case.Closed).Configure(config => config with
             {
                 ShrinkMode = ShrinkMode.All,
                 ReportMode = ReportMode.Trace | ReportMode.FinalTrace
             });

        var syncFailure = Assert.Throws<FalsifiableException>(() => Create(false).Run(123, 3.ExecutionsPerRun()));
        phases.Clear();
        var asyncFailure = await Assert.ThrowsAsync<FalsifiableException>(() => Bounded(Create(true).RunAsync(123, 3.ExecutionsPerRun(), TestContext.Current.CancellationToken)));
        var expected = Assert.IsType<CaseFile>(syncFailure.TheCaseFile);
        var actual = Assert.IsType<CaseFile>(asyncFailure.TheCaseFile);

        Assert.Equal(expected.GetFailure(), actual.GetFailure());
        Assert.Equal(expected.ShrinkCount, actual.ShrinkCount);
        Assert.True(actual.ShrinkCount > 0);
        Assert.Equal(expected.ExecutionCount, actual.ExecutionCount);
        Assert.Equal(expected.ActionCount, actual.ActionCount);
        Assert.Equal(expected.InputCount, actual.InputCount);
        Assert.Equal(expected.TraceCount, actual.TraceCount);
        Assert.Equal(expected.Distinctions, actual.Distinctions);
        Assert.True(phases.SetEquals(["initial", "replay", "trace", "final trace", "selection"]));
    }

    private static TaskCompletionSource Gate() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static CheckrState CreateState() => new(1, 100, 123, AutopsyProbe.Empty);
    private static Task<T> Bounded<T>(Task<T> task) => task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    private sealed class Counter { public int Value { get; set; } }
}
