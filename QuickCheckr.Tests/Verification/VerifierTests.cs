using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification;

public class VerifierTests
{
    [Fact]
    public async Task PropertySetterFailureRejectsCandidate()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("item", () => new PositiveItem { Amount = 42 });
        var property = typeof(PositiveItem).GetProperty(nameof(PositiveItem.Amount))!;

        using (state.Memory.NestedValue(1, new ObjectPropertyLens(property)))
        {
            var outcome = await new Verifier(state, PassingCheck).RunAsync(1, "item", -1);

            var rejected = Assert.IsType<ReplayOutcome.CandidateRejected>(outcome);
            Assert.IsType<ArgumentOutOfRangeException>(rejected.Exception);
        }
    }

    [Fact]
    public async Task MemoryLookupFailureIsNotTreatedAsCandidateRejection()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        var verifier = new Verifier(state, PassingCheck);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => verifier.RunAsync(2, "missing", 42).AsTask());
    }

    [Fact]
    public async Task ExceptionDuringReplayIsNotTreatedAsCandidateRejection()
    {
        var state = CreateState();
        state.Memory.StartNewExecution();
        state.Memory.StoreInput("number", () => 1);
        var verifier = new Verifier(
            state,
            _ => throw new InvalidOperationException("replay failed unexpectedly"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => verifier.RunAsync(1, "number", 2).AsTask());

        Assert.Equal("replay failed unexpectedly", exception.Message);
    }

    [Fact]
    public async Task RejectedTestimonyRollsBackEarlierAppliedTestimony()
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

        var outcome = await new Verifier(state, PassingCheck).TestimonyAsync(1, "item", 30);

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
