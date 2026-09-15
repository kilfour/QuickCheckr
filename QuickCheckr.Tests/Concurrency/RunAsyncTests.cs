using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Concurrency;

public class RunAsyncTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 4)]
    [InlineData(3, 12)]
    [InlineData(4, 4)]
    [InlineData(5, 1)]
    public async Task UnconfiguredOverloadsExecuteRequestedCountsAndReturnTheScript(int overload, int expected)
    {
        var calls = 0;
        var seeds = new List<int>();
        CheckrOf<Case> check = state =>
        {
            calls++;
            seeds.Add(state.FuzzrState.Seed);
            return CheckrResult.CaseOnly(state);
        };

        var result = await (overload switch
        {
            0 => check.RunAsync(TestContext.Current.CancellationToken),
            1 => check.RunAsync(3.Runs(), TestContext.Current.CancellationToken),
            2 => check.RunAsync(4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            3 => check.RunAsync(3.Runs(), 4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            4 => check.RunAsync(123, 4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            _ => check.RunAsync(123, TestContext.Current.CancellationToken)
        });

        Assert.Same(check, result);
        Assert.Equal(expected, calls);
        if (overload >= 4)
            Assert.All(seeds, seed => Assert.Equal(123, seed));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 4)]
    [InlineData(3, 12)]
    [InlineData(4, 4)]
    [InlineData(5, 1)]
    public async Task ConfiguredOverloadsExecuteRequestedCountsAndFileResults(int overload, int expected)
    {
        var calls = 0;
        var seeds = new List<int>();
        CheckrOf<Case> check = state =>
        {
            calls++;
            seeds.Add(state.FuzzrState.Seed);
            return CheckrResult.CaseOnly(state);
        };
        var custodian = new InMemoryCustodian([]);
        var configured = check.Configure(config => config with
        {
            FileAs = "async pass",
            Custodian = custodian
        });

        var result = await (overload switch
        {
            0 => configured.RunAsync(TestContext.Current.CancellationToken),
            1 => configured.RunAsync(3.Runs(), TestContext.Current.CancellationToken),
            2 => configured.RunAsync(4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            3 => configured.RunAsync(3.Runs(), 4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            4 => configured.RunAsync(123, 4.ExecutionsPerRun(), TestContext.Current.CancellationToken),
            _ => configured.RunAsync(123, TestContext.Current.CancellationToken)
        });

        Assert.Same(configured, result);
        Assert.Equal(expected, calls);
        Assert.IsType<Findings>(custodian.Records["async pass"]);
        Assert.Empty(custodian.Entries);
        if (overload >= 4)
            Assert.All(seeds, seed => Assert.Equal(123, seed));
    }

    [Fact]
    public async Task SeededRunsProduceTheSameInputsAsSynchronousRuns()
    {
        var values = new List<int>();
        var check =
            from input in Checkr.Input("input", Fuzzr.Int())
            from record in Checkr.Act("record", () => { values.Add(input); })
            select Case.Closed;

        check.Run(123, 8.ExecutionsPerRun());
        var expected = values.ToArray();
        values.Clear();
        await check.RunAsync(123, 8.ExecutionsPerRun(), TestContext.Current.CancellationToken);

        Assert.Equal(expected, values);
    }

    [Fact]
    public async Task ConfiguredFailurePreservesShrinkingAndFilesTheSeed()
    {
        var custodian = new InMemoryCustodian([]);
        var check =
            from input in Checkr.Input("input", Fuzzr.Constant(42))
            from expect in Checkr.Expect("less than ten", () => input < 10)
            select Case.Closed;
        var configured = check.Configure(config => config with
        {
            FileAs = "async failure",
            Custodian = custodian,
            ReportMode = ReportMode.Trace | ReportMode.FinalTrace
        });

        var syncFailure = Assert.Throws<FalsifiableException>(() => configured.Run(123));
        // Calling the API returns a task; failure is observed by awaiting it.
        var pending = configured.RunAsync(123, TestContext.Current.CancellationToken);
        var asyncFailure = await Assert.ThrowsAsync<FalsifiableException>(() => pending);
        var expected = Assert.IsType<CaseFile>(syncFailure.TheCaseFile);
        var actual = Assert.IsType<CaseFile>(asyncFailure.TheCaseFile);

        Assert.Equal(expected.GetFailure(), actual.GetFailure());
        Assert.Equal(expected.ShrinkCount, actual.ShrinkCount);
        Assert.Equal(expected.ExecutionCount, actual.ExecutionCount);
        Assert.Equal(expected.InputCount, actual.InputCount);
        Assert.Equal(expected.TraceCount, actual.TraceCount);
        Assert.Equal(123, actual.Seed);
        Assert.Same(actual, custodian.Records["async failure"]);
        var entry = Assert.Single(custodian.Entries);
        Assert.Equal(123, entry.Seed);
    }

    [Fact]
    public async Task UnconfiguredFailurePreservesTheOriginalActionException()
    {
        var check = Checkr.Act("throws", (Action)(() => throw new InvalidOperationException("after invocation")));

        var pending = check.RunAsync(TestContext.Current.CancellationToken);
        var failure = await Assert.ThrowsAsync<FalsifiableException>(() => pending);

        Assert.Contains("InvalidOperationException: after invocation", failure.Message);
        Assert.DoesNotContain("AggregateException", failure.Message);
    }
}
