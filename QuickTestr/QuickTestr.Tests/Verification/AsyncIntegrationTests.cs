using QuickCheckr;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickTestr.Tests.Verification;

public class AsyncIntegrationTests
{
    [Theory(Timeout = 10000)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SuspendingPropertiesShrinkAndReport(bool twoInputs, bool deliberate)
    {
        var calls = 0;
        async Task<bool> Property(int value)
        {
            await Task.Yield();
            calls++;
            return value < 10;
        }
        var one = Testr.Named("async property shrinking").For(Fuzzr.Constant(42));
        var two = Testr.Named("async property shrinking").For(Fuzzr.Constant(20), Fuzzr.Constant(22));
        Task Run() => (twoInputs, deliberate) switch
        {
            (false, false) => one.Assert(Property).RunAsync(123),
            (false, true) => one.Deliberate(x => x).Assert(Property).RunAsync(123),
            (true, false) => two.Assert((x, y) => Property(x + y)).RunAsync(123),
            _ => two.Deliberate((x, y) => x + y).Assert((x, y) => Property(x + y)).RunAsync(123)
        };

        var failure = await Assert.ThrowsAsync<FalsifiableException>(Run);
        var record = Assert.IsType<CaseFile>(failure.TheCaseFile);
        Assert.True(record.ShrinkCount > 0);
        Assert.True(calls > 1);
        Assert.Equal(123, record.Seed);
        Assert.Contains("async property shrinking", failure.Message);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false, false, 0)]
    [InlineData(false, false, 1)]
    [InlineData(false, false, 2)]
    [InlineData(false, true, 0)]
    [InlineData(false, true, 1)]
    [InlineData(false, true, 2)]
    [InlineData(true, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(true, false, 2)]
    [InlineData(true, true, 0)]
    [InlineData(true, true, 1)]
    [InlineData(true, true, 2)]
    public async Task OracleSidesAwaitSequentiallyThroughReplay(bool twoInputs, bool deliberate, int mode)
    {
        var order = new List<string>();
        int Expected(int value) { order.Add("expected"); return value; }
        int Actual(int value) { Assert.Equal("expected", order[^1]); order.Add("actual"); return value < 10 ? value : value + 1; }
        async Task<int> ExpectedAsync(int value) { await Task.Yield(); return Expected(value); }
        async Task<int> ActualAsync(int value) { await Task.Yield(); return Actual(value); }
        var one = Testr.Named("async oracle shrinking").For(Fuzzr.Constant(42));
        var two = Testr.Named("async oracle shrinking").For(Fuzzr.Constant(20), Fuzzr.Constant(22));
        Task Run()
        {
            if (twoInputs)
            {
                var expected = mode == 1
                    ? (deliberate ? two.Deliberate((x, y) => x + y).Expected((x, y) => Expected(x + y)) : two.Expected((x, y) => Expected(x + y)))
                    : (deliberate ? two.Deliberate((x, y) => x + y).Expected((x, y) => ExpectedAsync(x + y)) : two.Expected((x, y) => ExpectedAsync(x + y)));
                return (mode == 2 ? expected.Actual((x, y) => Actual(x + y)) : expected.Actual((x, y) => ActualAsync(x + y))).RunAsync(123);
            }
            var singleExpected = mode == 1
                ? (deliberate ? one.Deliberate(x => x).Expected(Expected) : one.Expected(Expected))
                : (deliberate ? one.Deliberate(x => x).Expected(ExpectedAsync) : one.Expected(ExpectedAsync));
            return (mode == 2 ? singleExpected.Actual(Actual) : singleExpected.Actual(ActualAsync)).RunAsync(123);
        }

        var failure = await Assert.ThrowsAsync<FalsifiableException>(Run);
        Assert.True(Assert.IsType<CaseFile>(failure.TheCaseFile).ShrinkCount > 0);
        Assert.True(order.Count > 2);
        Assert.Equal(0, order.Count % 2);
        for (var i = 0; i < order.Count; i += 2)
        {
            Assert.Equal("expected", order[i]);
            Assert.Equal("actual", order[i + 1]);
        }
    }

    [Theory(Timeout = 10000)]
    [InlineData(0, 100)]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    public async Task RunOverloadsPreserveCountsAndIdentity(int overload, int expectedCalls)
    {
        var calls = 0;
        var runner = Testr.Named("async counts").For(Fuzzr.Constant(42))
            .Assert(async _ => { await Task.Yield(); calls++; return true; });
        var result = await (overload switch
        {
            0 => runner.RunAsync(),
            1 => runner.RunAsync(123),
            _ => runner.RunAsync(3.Runs())
        });
        Assert.Same(runner, result);
        Assert.Equal(expectedCalls, calls);
    }

    [Fact(Timeout = 10000)]
    public async Task PostAwaitExceptionsKeepTheirOriginalMeaning()
    {
        async Task<int> Throw(int _) { await Task.Yield(); throw new InvalidOperationException("after suspension"); }
        await Testr.Named("matching exceptions").For(Fuzzr.Constant(42)).Expected(Throw).Actual(Throw).RunAsync(123);
        var failure = await Assert.ThrowsAsync<FalsifiableException>(() =>
            Testr.Named("throwing property").For(Fuzzr.Constant(42))
                .Assert(async x => await Throw(x) == 0).RunAsync(123));
        Assert.Contains("InvalidOperationException: after suspension", failure.Message);
        Assert.DoesNotContain("AggregateException", failure.Message);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancellationPropagatesWithoutFalsification(bool oracle)
    {
        using var cancellation = new CancellationTokenSource();
        async Task<int> Cancel(int input) { await Task.Yield(); cancellation.Cancel(); return input; }
        Task Run() => oracle
            ? Testr.Named("cancel oracle").For(Fuzzr.Constant(42)).Expected(Cancel).Actual(x => x).RunAsync(cancellation.Token)
            : Testr.Named("cancel property").For(Fuzzr.Constant(42)).Assert(async x => await Cancel(x) == x).RunAsync(cancellation.Token);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Run);
    }

    [Theory(Timeout = 10000)]
    [InlineData(0, 500, 20)]
    [InlineData(1, 3, 2)]
    [InlineData(2, 6, 4)]
    public async Task ModelRunOverloadsAwaitResourceDisposal(int overload, int operations, int resources)
    {
        var calls = 0;
        var disposed = 0;
        var runner = Testr.Named("async model cleanup")
            .Model(() => new Resource(() => disposed++))
            .Sut(() => new Resource(() => disposed++))
            .Operation("count", _ => { calls++; }, _ => { })
            .Observe("passes", (_, _) => true);
        var result = await (overload switch
        {
            0 => runner.RunAsync(),
            1 => runner.RunAsync(123, 3.ExecutionsPerRun()),
            _ => runner.RunAsync(2.Runs(), 3.ExecutionsPerRun())
        });
        Assert.Same(runner, result);
        Assert.Equal(operations, calls);
        Assert.Equal(resources, disposed);
    }

    private sealed class Resource(Action disposed) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() { await Task.Yield(); disposed(); }
    }
}
