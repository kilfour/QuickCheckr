using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification;

public class VerifierTests
{
    [Fact]
    public void PropertySetterFailureRejectsCandidate()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("item", () => new PositiveItem { Amount = 42 });
        var property = typeof(PositiveItem).GetProperty(nameof(PositiveItem.Amount))!;

        using (state.Memory.NestedValue(1, new ObjectPropertyLens(property)))
        {
            var outcome = new Verifier(state, PassingCheck).Run(1, "item", -1);

            var rejected = Assert.IsType<ReplayOutcome.CandidateRejected>(outcome);
            Assert.IsType<ArgumentOutOfRangeException>(rejected.Exception);
        }
    }

    [Fact]
    public void MemoryLookupFailureIsNotTreatedAsCandidateRejection()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        var verifier = new Verifier(state, PassingCheck);

        Assert.Throws<KeyNotFoundException>(() => verifier.Run(2, "missing", 42));
    }

    [Fact]
    public void ExceptionDuringReplayIsNotTreatedAsCandidateRejection()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("number", () => 1);
        var verifier = new Verifier(
            state,
            _ => throw new InvalidOperationException("replay failed unexpectedly"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => verifier.Run(1, "number", 2));

        Assert.Equal("replay failed unexpectedly", exception.Message);
    }

    [Fact]
    public void RejectedTestimonyRollsBackEarlierAppliedTestimony()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("item", () => new PositiveItem { Amount = 42 });
        var property = typeof(PositiveItem).GetProperty(nameof(PositiveItem.Amount))!;

        using (state.Memory.NestedValue(1, new ObjectPropertyLens(property)))
        {
            state.Memory.Testify(1, "item", 20);
            state.Memory.Testify(1, "item", -1);
        }

        var outcome = new Verifier(state, PassingCheck).Testimony(1, "item", 30);

        Assert.IsType<ReplayOutcome.CandidateRejected>(outcome);
        var current = Assert.IsType<PositiveItem>(
            state.Memory.GetCurrentInputValue(1, "item"));
        Assert.Equal(42, current.Amount);
    }

    private static CheckrState CreateState()
        => new(1, 100, 123, AutopsyProbe.Empty);

    private static ValueTask<CheckrResult<Case>> PassingCheck(CheckrState state)
        => CheckrResult.CaseOnly(state);

    private sealed class PositiveItem
    {
        private int amount;

        public int Amount
        {
            get => amount;
            init
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                amount = value;
            }
        }
    }
}
