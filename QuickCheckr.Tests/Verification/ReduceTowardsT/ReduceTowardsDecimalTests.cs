using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowardsDecimalTests
{
    [Fact]
    public async Task ReachesTargetWhenEveryCandidateFails()
    {
        var result = await TestReductionRunner.ForReducerAsync(Reduce.Towards(0m), 42.5m, _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0m, unbounded.Result);
    }

    [Fact]
    public async Task StopsAtClosestFailingDecimal()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(0m),
            1m,
            candidate => (decimal)candidate != 0m);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(0.0000000000000000000000000001m, stop.Result);
    }

    [Fact]
    public async Task StopsAtConfiguredPrecision()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(0m, precision: 2),
            1m,
            candidate => (decimal)candidate != 0m);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(0.01m, stop.Result);
    }

    [Fact]
    public async Task ReducesUpTowardsTarget()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(100m),
            1m,
            candidate => (decimal)candidate < 90m);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89.99999999999999999999999999m, stop.Result);
    }

    [Fact]
    public async Task ReducesUpAtConfiguredPrecision()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(100m, precision: 2),
            1m,
            candidate => (decimal)candidate < 90m);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89.99m, stop.Result);
    }

    [Fact]
    public async Task ZeroPrecisionBehavesLikeIntegerReduction()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(100m, precision: 0),
            1m,
            candidate => (decimal)candidate < 90m);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89m, stop.Result);
    }

    [Fact]
    public async Task HandlesFullDecimalRangeWithoutOverflow()
    {
        var result = await TestReductionRunner.ForReducerAsync(
            Reduce.Towards(decimal.MaxValue),
            decimal.MinValue,
            _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(decimal.MaxValue, unbounded.Result);
    }

    [Fact]
    public async Task TargetAsInitialValueIsUnbounded()
    {
        var result = await TestReductionRunner.ForReducerAsync(Reduce.Towards(12.34m), 12.34m, _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(12.34m, unbounded.Result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(29)]
    public void RejectsInvalidPrecision(int precision)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Reduce.Towards(0m, precision));
    }
}
