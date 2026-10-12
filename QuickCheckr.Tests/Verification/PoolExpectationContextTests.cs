using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Verification;

public class PoolExpectationContextTests
{
    private static CheckrState State()
    {
        var state = new CheckrState(1, 100, 123, AutopsyProbe.Empty);
        state.Memory.StartNewExecution();
        state.Memory.StoreInPool(1, 42);
        state.Memory.StoreInPool(2, 7);
        return state;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Each_reports_only_failed_items_and_their_messages(bool asynchronous)
    {
        var state = State();
        var check = asynchronous
            ? Trackr.PoolExpectEach<int>("each", async (item, context, token) =>
            {
                await Task.Yield();
                return context.Equal(42, item);
            })
            : Trackr.PoolExpectEach<int>("each", (item, context) => context.Equal(42, item));
        await check(state);
        Assert.Equal(new[] { "Int32-2", "Expected: 42", "Actual: 7" }, state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task When_filters_before_evaluating_context_callback()
    {
        var state = State();
        var calls = 0;
        await Trackr.PoolExpectEachWhen<int>("filtered", async (item, token) =>
        {
            await Task.Yield();
            return item == 7;
        }, (item, context) =>
        {
            calls++;
            return context.Fail("first", "second");
        })(state);
        Assert.Equal(1, calls);
        Assert.Equal(new[] { "Int32-2", "first", "second" }, state.EvidenceRecorder.FailingExpectation.Message);
    }

    [Fact]
    public async Task No_matching_items_does_not_invoke_expectation()
    {
        var state = State();
        var calls = 0;
        await Trackr.PoolExpectEachWhen<int>("filtered", _ => false,
            (item, context) => { calls++; return context.Fail("unexpected"); })(state);
        Assert.Equal(0, calls);
        Assert.False(state.EvidenceRecorder.Failed());
    }

    [Fact]
    public async Task Related_pool_reports_context_messages()
    {
        var state = State();
        state.Memory.StoreInPool(3, "child");
        await Trackr.PoolExpectEach<int, string>("related", (parent, child) => parent == 42,
            async (parent, children, context, token) =>
            {
                await Task.Yield();
                return context.Equal(2, children.Count);
            })(state);
        Assert.Equal(new[] { "Expected: 2", "Actual: 1" }, state.EvidenceRecorder.FailingExpectation.Message);
    }
}
