using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.Tests.Verification;

public class PoolConditionTests
{
    private static CheckrState State()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        return state;
    }

    [Fact]
    public async Task Two_pool_condition_is_deferred_and_composes_with_other_guards()
    {
        var state = State();
        var calls = 0;
        var condition = Poolr.Has<int>().AndWhen<int, string>((numbers, words) =>
        {
            calls++;
            return numbers.Count == words.Count;
        });
        var option = (await condition.Then(Checkr.Trace("ready", _ => []))(state)).Value;
        Assert.False(option.Condition());
        Assert.Equal(0, calls);
        state.Memory.StoreInPool(1, 42);
        Assert.False(option.Condition());
        state.Memory.StoreInPool(2, "match");
        Assert.True(option.Condition());
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Pair_option_only_selects_matching_values_and_supports_async_callbacks()
    {
        var state = State();
        state.Memory.StoreInPool(1, 42);
        state.Memory.StoreInPool(2, 7);
        state.Memory.StoreInPool(3, "42");
        state.Memory.StoreInPool(4, "other");
        var calls = 0;
        var option = (await Poolr.When<int, string>("pair",
            (number, word) => number.ToString() == word,
            (number, word) => async current =>
            {
                await Task.Yield();
                Assert.Equal(42, number);
                Assert.Equal("42", word);
                calls++;
                return CheckrResult.CaseOnly(current);
            })(state)).Value;
        Assert.Equal(0, calls);
        Assert.True(option.Condition());
        await option.Checkr(state);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Pair_availability_uses_current_pool_contents()
    {
        var state = State();
        var option = (await Poolr.When<int, string>("pair",
            (number, word) => number.ToString() == word,
            (_, _) => Checkr.Trace("selected", _ => []))(state)).Value;
        Assert.False(option.Condition());
        state.Memory.StoreInPool(1, 42);
        state.Memory.StoreInPool(2, "42");
        Assert.True(option.Condition());
        state.Memory.GetPoolDictionary<string>()[2] = "different";
        Assert.False(option.Condition());
        await Assert.ThrowsAsync<NoMatchingPoolElementException>(async () => await option.Checkr(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Replay_aborts_if_selected_pair_is_missing_or_no_longer_matches(bool missing)
    {
        var state = State();
        state.Memory.StoreInPool(1, 42);
        state.Memory.StoreInPool(2, "42");
        var calls = 0;
        var option = (await Poolr.When<int, string>("pair",
            (number, word) => number.ToString() == word,
            (_, _) => current =>
            {
                calls++;
                return CheckrResult.CaseOnly(current);
            })(state)).Value;
        await option.Checkr(state);
        state.Memory.GotoExecution(1);
        if (missing)
            state.Memory.GetPoolDictionary<string>().Remove(2);
        else
            state.Memory.GetPoolDictionary<string>()[2] = "different";
        // Another valid pair must not replace the previously selected identity during replay.
        state.Memory.StoreInPool(3, "42");
        Assert.True(option.Condition());
        await option.Checkr(state);
        Assert.Equal(1, calls);
        Assert.True(state.ExecutionAborted);
    }
}