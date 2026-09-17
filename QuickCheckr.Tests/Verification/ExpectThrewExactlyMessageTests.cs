using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.Tests.Verification;

public class ExpectThrewExactlyMessageTests
{
    [Theory]
    [InlineData("expected", true)]
    [InlineData("Expected", false)]
    [InlineData("expected ", false)]
    [InlineData("different", false)]
    public async Task ChecksExactMessageAndReportsBothValues(string actual, bool passes)
    {
        var state = State();
        var calls = 0;
        var check = Checkr.ExpectThrewExactly<InvalidOperationException>("throws",
            new DelayedResult(new InvalidOperationException(actual)), () => { calls++; return "expected"; });
        Assert.Equal(0, calls);
        await check(state);
        Assert.Equal(1, calls);
        Assert.Equal(!passes, state.EvidenceRecorder.Failed());
        if (!passes)
        {
            Assert.Contains($"Message:  {actual}", state.EvidenceRecorder.FailingExpectation.Message);
            Assert.Contains("Expected message: expected", state.EvidenceRecorder.FailingExpectation.Message);
        }
    }

    [Fact]
    public async Task OmittingMessageKeepsTypeOnlyBehavior()
    {
        var state = State();
        await Checkr.ExpectThrewExactly<InvalidOperationException>("throws",
            new DelayedResult(new InvalidOperationException("anything")))(state);
        Assert.Equal(1, state.EvidenceRecorder.PassedExpectations["throws"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MatchingMessageDoesNotPermitWrongTypeOrMissingException(bool missing)
    {
        var state = State();
        var result = missing ? new DelayedResult() : new DelayedResult(new ArgumentException("expected"));
        await Checkr.ExpectThrewExactly<Exception>("throws", result, () => "expected")(state);
        Assert.True(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task SkippedReplayDoesNotEvaluateMessage()
    {
        var state = State();
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("target", []), 1);
        state.StartReplay();
        await Checkr.ExpectThrewExactly<Exception>("other", new DelayedResult(),
            () => throw new InvalidOperationException("must not run"))(state);
        Assert.Null(state.EvidenceRecorder.ReplayException);
    }

    [Fact]
    public async Task ActiveReplayRejectsThrowingMessageFactory()
    {
        var state = State();
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("throws", []), 1);
        state.StartReplay();
        await Checkr.ExpectThrewExactly<Exception>("throws", new DelayedResult(),
            () => throw new InvalidOperationException("message factory"))(state);
        Assert.IsType<InvalidOperationException>(state.EvidenceRecorder.ReplayException);
    }

    private static CheckrState State()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty, TestContext.Current.CancellationToken);
        state.Memory.StartNewExecution();
        return state;
    }
}
