using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification;

public class ActionAbortTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedActionSkipsDependentPoolInsertionAndProjection(bool asyncAction)
    {
        var state = CreateState();
        var error = new InvalidOperationException("insert failed");
        var inserted = false;
        var projected = false;
        var action = asyncAction
            ? Checkr.Act<string>("insert", async () => { await Task.Yield(); throw error; })
            : Checkr.Act<string>("insert", (Func<string>)(() => throw error));
        var check = from value in action
                    from pool in Poolr.Add("item", Checkr.Capture(() => { inserted = true; return value; }))
                    select Project();
        Case Project() { projected = true; return Case.Closed; }

        await check(state);

        Assert.False(inserted);
        Assert.False(projected);
        Assert.Empty(state.Memory.GetPoolIds<string>());
        Assert.Same(error, state.EvidenceRecorder.Exception);
        Assert.True(state.ExecutionAborted);
    }

    [Fact]
    public async Task ReplayRejectsActionExceptionAndNextCandidateCanContinue()
    {
        var state = CreateState();
        state.Memory.StoreInput("number", () => 1);
        var continued = 0;
        var check = from number in Checkr.Input("number", QuickFuzzr.Fuzzr.OneOf(1))
                    from value in Checkr.Act("insert", () => number < 0
                        ? throw new InvalidOperationException("invalid candidate") : "order")
                    from pool in Poolr.Add("order", Checkr.Capture(() => { continued++; return value; }))
                    select Case.Closed;
        var verifier = new Verifier(state, check);

        var rejected = await verifier.RunAsync(1, "number", -1);
        Assert.IsType<ReplayOutcome.CandidateRejected>(rejected);
        Assert.Equal(0, continued);

        var passed = await verifier.RunAsync(1, "number", 1);
        Assert.IsType<ReplayOutcome.Passed>(passed);
        Assert.Equal(1, continued);
        Assert.False(state.ExecutionAborted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CarefullyCapturedExceptionsAllowContinuation(bool asyncAction)
    {
        var state = CreateState();
        var error = new InvalidOperationException("expected");
        var continued = false;
        var action = asyncAction
            ? Checkr.ActCarefully<int>("insert", async () => { await Task.Yield(); throw error; })
            : Checkr.ActCarefully<int>("insert", (Func<int>)(() => throw error));

        await action.Select(result =>
        {
            Assert.Same(error, result.Exception);
            continued = true;
            return Case.Closed;
        })(state);

        Assert.True(continued);
        Assert.False(state.ExecutionAborted);
    }

    private static CheckrState CreateState()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty,
            TestContext.Current.CancellationToken);
        state.Memory.StartNewExecution();
        return state;
    }
}
