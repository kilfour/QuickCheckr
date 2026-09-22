using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.Authoring.ThePress;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Verification;

public class CheckrExpectEqualsTests
{
    [Fact]
    public void Equal_values_pass()
    {
        var article = Journalist.Publish(
            Checkr.Expect("answer", a => a.Equal(42, 42)),
            checkr => checkr.Run());

        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(["answer"], article.PassedExpectationLabels());
    }

    [Fact]
    public void Different_values_are_reported_using_the_configured_formatter()
    {
        var checkr =
            from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(value => $"<{value}>"))
            from expectation in Checkr.Expect("answer", a => a.Equal(41, 42))
            select Case.Closed;

        var article = Journalist.Publish(checkr, configured => configured.Run());

        Assert.Equal("answer", article.FailureDescription());
        Assert.Equal(["Expected: <41>", "Actual: <42>"], article.FailingExpectationMessages());
    }

    [Fact]
    public void Null_values_are_compared()
    {
        string? expected = null;
        string? actual = null;

        var article = Journalist.Publish(
            Checkr.Expect("nullable", a => a.Equal(expected, actual)),
            checkr => checkr.Run());

        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(["nullable"], article.PassedExpectationLabels());
    }

    [Fact]
    public void Deferred_values_pass_and_support_nulls()
    {
        var article = Journalist.Publish(
            Checkr.Expect("nullable", a => a.Equal<string>(null, null)),
            checkr => checkr.Run());

        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(["nullable"], article.PassedExpectationLabels());
    }

    [Fact]
    public async Task Deferred_values_are_evaluated_once_and_report_the_compared_values()
    {
        var expectedCalls = 0;
        var actualCalls = 0;
        var check = Checkr.Expect("answer", a => a.Equal(++expectedCalls, ++actualCalls + 1));
        Assert.Equal(0, expectedCalls);
        Assert.Equal(0, actualCalls);
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.Editor = Please.AllowMe().ToReplace<int>(value => $"<{value}>");

        await check(state);

        Assert.Equal(1, expectedCalls);
        Assert.Equal(1, actualCalls);
        Assert.Equal(["Expected: <1>", "Actual: <2>"], state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task Skipped_expectation_does_not_evaluate_either_factory()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.EvidenceRecorder.RecordFailure(new ArgumentException("invalid color"));
        state.StartReplay();
        var calls = 0;
        string Value()
        {
            calls++;
            throw new NullReferenceException("response is missing");
        }

        await Checkr.Expect("board name", a => a.Equal(Value(), Value()))(state);

        Assert.Equal(0, calls);
        Assert.Null(state.EvidenceRecorder.ReplayException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Factory_exception_in_active_expectation_rejects_shrink_candidate(bool expectedThrows)
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("name", () => "original");
        state.EvidenceRecorder.ExpectationFailed(new ExpectationFailure("board name", []), 1);
        string Throws() => throw new NullReferenceException("response is missing");
        var check = Checkr.Expect("board name",
            a => a.Equal(
            expectedThrows ? Throws() : "expected",
            expectedThrows ? "actual" : Throws()));

        var outcome = await new Verifier(state, check).RunAsync(1, "name", "a");

        var rejected = Assert.IsType<ReplayOutcome.CandidateRejected>(outcome);
        Assert.IsType<NullReferenceException>(rejected.Exception);
    }
}
