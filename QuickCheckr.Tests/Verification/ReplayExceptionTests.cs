using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.Tests.Verification;

public class ReplayExceptionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DifferentCaughtExceptionRejectsCandidateAndNextReplayResets(bool testimony)
    {
        var state = CreateState();
        var check = NameCheck(state, _ => false);
        var verifier = new Verifier(state, check);

        var rejected = Assert.IsType<ReplayOutcome.CandidateRejected>(
            await Replay(verifier, testimony, ""));
        Assert.IsType<InvalidOperationException>(rejected.Exception);
        Assert.IsType<ReplayOutcome.FailurePreserved>(await Replay(verifier, testimony, "a"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SuccessfulReplayPasses(bool testimony)
    {
        var state = CreateState();
        var verifier = new Verifier(state, NameCheck(state, name => name == "a"));
        Assert.IsType<ReplayOutcome.Passed>(await Replay(verifier, testimony, "a"));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DifferentExceptionTakesPrecedenceOverPreservedFailure(bool testimony, bool originalFirst)
    {
        var state = CreateState();
        CheckrOf<Case> check = s =>
        {
            if (originalFirst) s.EvidenceRecorder.RecordFailure(ColorFailure());
            s.EvidenceRecorder.RecordFailure(new InvalidOperationException("invalid name"));
            if (!originalFirst) s.EvidenceRecorder.RecordFailure(ColorFailure());
            return CheckrResult.CaseOnly(s);
        };
        Assert.IsType<ReplayOutcome.CandidateRejected>(
            await Replay(new Verifier(state, check), testimony, "a"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CrashedExpectationRejectsCandidate(bool testimony)
    {
        var state = CreateState();
        CheckrOf<Case> check = s =>
        {
            s.EvidenceRecorder.ExpectationCrashed("expectation", new NullReferenceException());
            return CheckrResult.CaseOnly(s);
        };
        Assert.IsType<ReplayOutcome.CandidateRejected>(
            await Replay(new Verifier(state, check), testimony, "a"));
    }

    [Fact]
    public async Task ValidNamesPreservingColorFailureAreIrrelevant()
    {
        var state = CreateState();
        var classification = await Classify(state, NameCheck(state, _ => false));
        Assert.IsType<InputAssumedIrrelevant>(classification);
        // The accepted witness remains valid when testimony is applied.
        Assert.IsType<ReplayOutcome.FailurePreserved>(
            await new Verifier(state, NameCheck(state, _ => false)).TestimonyAsync(1, "Name", "a"));
    }

    [Fact]
    public async Task ValidNameThatPassesRemainsRelevant()
    {
        var state = CreateState();
        Assert.IsType<InputRelevant>(await Classify(state, NameCheck(state, name => name == "a")));
    }

    [Fact]
    public async Task AllRejectedReplacementsRemainRelevant()
    {
        var state = CreateState();
        Assert.IsType<InputRelevant>(await Classify(state, NameCheck(state, _ => false, rejectAll: true)));
    }

    private static ValueTask<InputClassification> Classify(CheckrState state, CheckrOf<Case> check)
        => new ClassifierSelector().ClassifyDefaultAsync(
            new ShrinkingInfo(1, "Name", "original", "Name", typeof(string)),
            new ShrinkingContext(state, check, false));

    private static CheckrState CreateState()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("Name", () => "original");
        state.EvidenceRecorder.RecordFailure(ColorFailure());
        return state;
    }

    private static CheckrOf<Case> NameCheck(CheckrState state, Func<string, bool> passes, bool rejectAll = false)
        => from action in Checkr.Act("Create", () =>
        {
            var name = (string?)state.Memory.GetCurrentInputValue(1, "Name");
            if (rejectAll || string.IsNullOrWhiteSpace(name) || name.Length > 100)
                throw new InvalidOperationException("invalid name");
            if (!passes(name)) throw ColorFailure();
            return true;
        })
        select Case.Closed;

    private static ArgumentException ColorFailure() => new("invalid color");

    private static ValueTask<ReplayOutcome> Replay(Verifier verifier, bool testimony, string name)
        => testimony ? verifier.TestimonyAsync(1, "Name", name) : verifier.RunAsync(1, "Name", name);
}
