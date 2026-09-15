using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Concurrency;

public class AsyncActionTests
{
    [Theory]
    [InlineData("act")]
    [InlineData("act void")]
    [InlineData("act when")]
    [InlineData("act when void")]
    [InlineData("capture")]
    [InlineData("perform")]
    [InlineData("careful")]
    [InlineData("careful void")]
    [InlineData("careful when")]
    [InlineData("shrinkable")]
    [InlineData("shrinkable void")]
    [InlineData("shrinkable careful")]
    [InlineData("shrinkable careful void")]
    public async Task HelpersWaitForSuspendedOperations(string operation)
    {
        var release = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var continued = false;
        Func<Task<int>> value = () => release.Task;
        Func<Task> action = async () => { await value(); };
        var step = operation switch
        {
            "act" => Checkr.Act("act", value).Select(v => { Assert.Equal(42, v); return Case.Closed; }),
            "act void" => Checkr.Act("act", action),
            "act when" => Checkr.ActWhen("act", () => true, value).Select(v => { Assert.Equal(42, v.Value); return Case.Closed; }),
            "act when void" => Checkr.ActWhen("act", () => true, action),
            "capture" => Checkr.Capture(value).Select(v => { Assert.Equal(42, v); return Case.Closed; }),
            "perform" => Checkr.Perform(action),
            "careful" => Checkr.ActCarefully("act", value).Select(v => { Assert.Equal(42, v.Value); return Case.Closed; }),
            "careful void" => Checkr.ActCarefully("act", action).Select(v => { Assert.False(v.Threw); return Case.Closed; }),
            "careful when" => Checkr.ActCarefullyWhen("act", () => true, value).Select(v => { Assert.Equal(42, v.Value); return Case.Closed; }),
            "shrinkable" => Checkr.ShrinkableAct("act", value).Select(v => { Assert.Equal(42, v.Result); return Case.Closed; }),
            "shrinkable void" => Checkr.ShrinkableAct("act", action).Select(v => { Assert.True(v.HasExecuted); return Case.Closed; }),
            "shrinkable careful" => Checkr.ShrinkableActCarefully("act", value).Select(v => { Assert.Equal(42, v.Result.Value); return Case.Closed; }),
            _ => Checkr.ShrinkableActCarefully("act", action).Select(v => { Assert.False(v.Result.Threw); return Case.Closed; })
        };
        var check = from run in step
                    from after in Checkr.Act("after", () => { continued = true; })
                    select Case.Closed;
        var pending = check.RunAsync(TestContext.Current.CancellationToken);
        try { Assert.False(pending.IsCompleted); Assert.False(continued); }
        finally { release.TrySetResult(42); }
        await pending.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        Assert.True(continued);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CarefullyCapturesExceptionsThrownAfterSuspension(bool shrinkable)
    {
        var error = new InvalidOperationException("after suspension");
        Func<Task<int>> action = async () => { await Task.Yield(); throw error; };
        var step = shrinkable
            ? Checkr.ShrinkableActCarefully("act", action).Select(v => v.Result)
            : Checkr.ActCarefully("act", action);
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();

        var result = await step(state);

        Assert.True(result.Value.Threw);
        Assert.Same(error, result.Value.Exception);
    }

    [Fact]
    public async Task FailingAsyncActionsAreReportedWithoutAggregateException()
    {
        var calls = 0;
        var check = Checkr.Act("throws", async () =>
        {
            await Task.Yield();
            calls++;
            throw new InvalidOperationException("after suspension");
        });

        var error = await Assert.ThrowsAsync<FalsifiableException>(() => check.RunAsync(TestContext.Current.CancellationToken));

        Assert.Contains("InvalidOperationException: after suspension", error.Message);
        Assert.DoesNotContain("AggregateException", error.Message);
        Assert.True(calls > 1); // The failing action is invoked again during replay.
    }

    [Fact]
    public async Task ConditionalAndRemovedActionsDoNotInvokeFactories()
    {
        var calls = 0;
        Func<Task<int>> action = async () => { calls++; await Task.Yield(); return 42; };
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        await Checkr.ActWhen("conditional", () => false, action)(state);
        await Checkr.ActCarefullyWhen("careful", () => false, action)(state);
        state.StartReplay(); // No recorded shrinkable actions: both are skipped.
        var normal = await Checkr.ShrinkableAct("removed", action)(state);
        var careful = await Checkr.ShrinkableActCarefully("removed carefully", action)(state);

        Assert.Equal(0, calls);
        Assert.False(normal.Value.HasExecuted);
        Assert.False(careful.Value.HasExecuted);
    }

    [Fact]
    public async Task AsyncActionsShrinkAndDisposeFreshResourcesOnEveryReplay()
    {
        var resources = new List<Resource>();
        var phases = new HashSet<string>();
        CheckrOf<Case> note = state =>
        {
            if (state.Memory.Tracing) phases.Add("trace");
            if (state.Memory.FinalTrace) phases.Add("final trace");
            if (state.Memory.Distinguishing) phases.Add("selection");
            return CheckrResult.CaseOnly(state);
        };
        var check =
            from resource in Trackr.Stashed(() =>
            {
                Assert.All(resources, r => Assert.True(r.Disposed));
                var next = new Resource(); resources.Add(next); return next;
            })
            from input in Checkr.Input("input", Fuzzr.Constant(42))
            from increment in Checkr.ShrinkableAct("increment", async () => { await Task.Yield(); resource.Count++; })
            from irrelevant in Checkr.ShrinkableAct("irrelevant", async () => { await Task.Yield(); })
            from observation in Checkr.ActCarefully("read", async () => { await Task.Yield(); return resource.Count; })
            from distinction in Checkr.Distinguish(() => "category")
            from phase in note
            from expect in Checkr.Expect("below threshold", () => observation.Value < 2 || input < 10)
            select Case.Closed;
        var configured = check.Configure(c => c with { ShrinkMode = ShrinkMode.All, ReportMode = ReportMode.Trace | ReportMode.FinalTrace });

        var failure = await Assert.ThrowsAsync<FalsifiableException>(() => configured.RunAsync(123, 3.ExecutionsPerRun(), TestContext.Current.CancellationToken));
        var record = Assert.IsType<CaseFile>(failure.TheCaseFile);

        Assert.Equal("below threshold", record.GetFailure());
        Assert.True(record.ShrinkCount > 0);
        Assert.True(resources.Count > 2);
        Assert.All(resources, r => Assert.True(r.Disposed));
        Assert.True(phases.SetEquals(["trace", "final trace", "selection"]));
    }

    private sealed class Resource : IAsyncDisposable
    {
        public int Count { get; set; }
        public bool Disposed { get; private set; }
        public async ValueTask DisposeAsync() { Assert.False(Disposed); await Task.Yield(); Disposed = true; }
    }
}
