using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification;

public class AsyncExpectationCompletionTests
{
    private static async Task<int> Number(int number) { await Task.Yield(); return number; }
    private static async Task<bool> Bool(bool value) { await Task.Yield(); return value; }
    private static CheckrState State()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty, TestContext.Current.CancellationToken);
        state.Memory.StartNewExecution();
        return state;
    }

    [Fact]
    public async Task EqualityAwaitsTaskValuesRatherThanComparingTaskObjects()
    {
        var state = State();
        await Checkr.Expect("equal", async expectation =>
            expectation.Equal(await Number(1000), await Number(1000)))(state);
        Assert.Equal(1, state.EvidenceRecorder.PassedExpectations["equal"]);
        await Checkr.Expect("different", async expectation =>
            expectation.Equal(await Number(1000), await Number(1001)))(state);
        Assert.Equal(["Expected: 1000", "Actual: 1001"], state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task EqualityComposesSynchronousAndAsynchronousValues()
    {
        var state = State();
        await Checkr.Expect("sync async", async expectation =>
            expectation.Equal(42, await Number(42)))(state);
        await Checkr.Expect("async sync", async expectation =>
            expectation.Equal(await Number(42), 42))(state);
        await Checkr.Expect("with token", async (expectation, token) =>
        {
            Assert.Equal(TestContext.Current.CancellationToken, token);
            return expectation.Equal(await Number(42), await Number(42));
        })(state);
        Assert.Equal(3, state.EvidenceRecorder.PassedExpectations.Count);
    }

    [Fact]
    public async Task EqualityCapturesEachAwaitedValueOnceForFailureReport()
    {
        var state = State();
        var expectedCalls = 0;
        var actualCalls = 0;
        await Checkr.Expect("values", async expectation =>
        {
            await Task.Yield();
            var expected = ++expectedCalls;
            await Task.Yield();
            var actual = ++actualCalls + 1;
            return expectation.Equal(expected, actual);
        })(state);
        Assert.Equal(1, expectedCalls);
        Assert.Equal(1, actualCalls);
        Assert.Equal(["Expected: 1", "Actual: 2"], state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task ConditionalFalseSkipsConditionAndMessagesWithoutRecordingPass()
    {
        var state = State();
        var calls = 0;
        await Checkr.ExpectWhen("conditional", () => Bool(false),
            async expectation =>
            {
                calls++;
                await Task.Yield();
                return expectation.Fail(Message());
            })(state);
        string Message() { calls++; return "unused"; }
        Assert.Equal(0, calls);
        Assert.Empty(state.EvidenceRecorder.PassedExpectations);
        Assert.False(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task ConditionalMixedCallbacksAndMessagesWork()
    {
        var state = State();
        await Checkr.ExpectWhen("a", () => true, () => Bool(true))(state);
        await Checkr.ExpectWhen("b", () => Bool(true), () => Bool(true))(state);
        await Checkr.ExpectWhen("c", token => Bool(true), async (expectation, token) =>
            await Bool(false) ? expectation.Pass() : expectation.Fail("reason"))(state);
        Assert.Equal(2, state.EvidenceRecorder.PassedExpectations.Count);
        Assert.Equal(["reason"], state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SkippedConditionalAndPoolFilterNeverInvokePredicates(bool asyncPredicate)
    {
        var state = State();
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("target", []), 1);
        state.StartReplay();
        state.Memory.StoreInPool(1, 1);
        var calls = 0;
        bool Predicate() { calls++; throw new InvalidOperationException(); }
        async Task<bool> AsyncPredicate() { await Task.Yield(); return Predicate(); }
        var when = asyncPredicate
            ? Checkr.ExpectWhen("when", AsyncPredicate, async expectation =>
            {
                await Task.Yield();
                return expectation.Pass();
            })
            : Checkr.ExpectWhen("when", Predicate, async expectation =>
            {
                await Task.Yield();
                return expectation.Pass();
            });
        var pool = asyncPredicate ? Trackr.PoolExpectEachWhen<int>("pool", _ => AsyncPredicate(), _ => true)
            : Trackr.PoolExpectEachWhen<int>("pool", _ => Predicate(), _ => true);
        await when(state);
        await pool(state);
        await Checkr.Expect("equal", async expectation =>
            expectation.Equal(await Number(calls++), await Number(calls++)))(state);
        Assert.Equal(0, calls);
        Assert.Null(state.EvidenceRecorder.ReplayException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PoolPredicateExceptionsAreGuardedDuringReplay(bool asynchronous)
    {
        var state = State();
        state.Memory.StoreInput("input", () => 1);
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("pool", []), 1);
        CheckrOf<Case> check = async s =>
        {
            s.Memory.StoreInPool(1, 1);
            bool Throw() => throw new InvalidOperationException("filter");
            var pool = asynchronous
                ? Trackr.PoolExpectEachWhen<int>("pool", async _ => { await Task.Yield(); return Throw(); }, _ => true)
                : Trackr.PoolExpectEachWhen<int>("pool", _ => Throw(), _ => Bool(true));
            return await pool(s);
        };
        var outcome = await new Verifier(state, check).RunAsync(1, "input", 2);
        Assert.IsType<ReplayOutcome.CandidateRejected>(outcome);
    }

    [Fact]
    public async Task AsyncFilterRunsSequentiallyBeforeSelectedConditions()
    {
        var state = State();
        state.Memory.StoreInPool(1, 1);
        state.Memory.StoreInPool(2, 2);
        List<string> calls = [];
        await Trackr.PoolExpectEachWhen<int>("filtered", async item =>
        { calls.Add($"filter {item}"); await Task.Yield(); return item == 2; }, async item =>
        { calls.Add($"check {item}"); await Task.Yield(); return false; })(state);
        Assert.Equal(["filter 1", "filter 2", "check 2"], calls);
        Assert.Equal(["Int32-2"], state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task AsyncBelongsToSupportsSynchronousExpectationAndEmptyMatches()
    {
        var state = State();
        state.Memory.StoreInPool(1, "one");
        state.Memory.StoreInPool(2, "empty");
        state.Memory.StoreInPool(1, 1);
        List<string> checkedParents = [];
        await Trackr.PoolExpectEach<string, int>("related", async (parent, child) =>
        { await Task.Yield(); return parent == "one"; }, (parent, children) =>
        {
            checkedParents.Add(parent);
            return children.Count == (parent == "one" ? 1 : 0);
        })(state);
        Assert.Equal(["one", "empty"], checkedParents);
        Assert.False(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task SkippedRelatedExpectationDoesNotInvokeAsyncBelongsTo()
    {
        var state = State();
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("target", []), 1);
        state.StartReplay();
        state.Memory.StoreInPool(1, "one");
        state.Memory.StoreInPool(1, 1);
        var calls = 0;
        await Trackr.PoolExpectEach<string, int>("related", async (_, _) =>
        { calls++; await Task.Yield(); return true; }, (_, _) => true)(state);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task RunLevelExpectAndVerifyOnlyEvaluateOnFinalExecution()
    {
        var state = new CheckrState(2, 100, 123, AutopsyProbe.Empty, TestContext.Current.CancellationToken);
        var calls = 0;
        async Task<bool> Condition() { calls++; await Task.Yield(); return true; }
        state.Memory.StartNewExecution();
        await Trackr.Expect("expect", Condition)(state);
        await Trackr.Verify("verify", Condition)(state);
        Assert.Equal(0, calls);
        state.Memory.StartNewExecution();
        await Trackr.Expect("expect", Condition)(state);
        await Trackr.Verify("verify", Condition)(state);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task LabelledVerificationAwaitsEveryConditionAndReportsFailures()
    {
        var state = State();
        List<int> order = [];
        await Trackr.Verify("verify",
            ("first", async () => { order.Add(1); await Task.Yield(); return false; }
        ),
            ("second", async () => { order.Add(2); await Task.Yield(); return true; }
        ),
            ("third", async () => { order.Add(3); await Task.Yield(); return false; }
        ))(state);
        Assert.Equal([1, 2, 3], order);
        Assert.Equal("verify: first, third", state.EvidenceRecorder.FailingVerification.Key);
    }

    [Fact]
    public async Task VerificationPreservesExceptionPropagationAfterAwait()
    {
        var state = State();
        var check = Trackr.Verify("verify", async () => { await Task.Yield(); throw new InvalidOperationException("verify"); });
        await Assert.ThrowsAsync<InvalidOperationException>(() => check(state).AsTask());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StopWhenProvenAwaitsSuccessAndStopsRunnerEarly(bool asynchronous)
    {
        var calls = 0;
        var check = asynchronous ? Trackr.StopWhenProven("eventually", async () => { await Task.Yield(); return ++calls == 2; }) : Trackr.StopWhenProven("eventually", () => ++calls == 2);
        await new CheckrRunner(check, new CheckrRunnerConfig(CheckrConfig.Default()), 10, AutopsyProbe.Empty).RunAsync();
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task CancellationInEqualityStopsBeforeActualFactory()
    {
        using var cancellation = new CancellationTokenSource();
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty, cancellation.Token);
        state.Memory.StartNewExecution();
        var actualCalls = 0;
        var check = Checkr.Expect("equal", async (expectation, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            await Task.Yield();
            cancellation.Cancel();
            var expected = 1;
            token.ThrowIfCancellationRequested();
            var actual = ++actualCalls;
            return expectation.Equal(expected, actual);
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => check(state).AsTask());
        Assert.Equal(0, actualCalls);
    }
}
