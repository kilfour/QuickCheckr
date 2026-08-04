using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowardsDoubleTests
{
    [Fact]
    public void ReachesTargetWhenEveryCandidateFails()
    {
        var result = TestReductionRunner.ForReducer(Reduce.Towards(0d), 42.5d, _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0d, unbounded.Result);
    }

    [Fact]
    public void StopsAtConfiguredPrecision()
    {
        var result = TestReductionRunner.ForReducer(
            Reduce.Towards(0d, precision: 2),
            1d,
            candidate => (double)candidate != 0d);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(0.01d, stop.Result);
    }

    [Fact]
    public void ReducesUpAtConfiguredPrecision()
    {
        var result = TestReductionRunner.ForReducer(
            Reduce.Towards(100d, precision: 2),
            1d,
            candidate => (double)candidate < 90d);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89.99d, stop.Result);
    }

    [Fact]
    public void ZeroPrecisionBehavesLikeIntegerReduction()
    {
        var result = TestReductionRunner.ForReducer(
            Reduce.Towards(100d, precision: 0),
            1d,
            candidate => (double)candidate < 90d);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89d, stop.Result);
    }

    [Fact]
    public void HandlesFullFiniteRange()
    {
        var result = TestReductionRunner.ForReducer(
            Reduce.Towards(double.MaxValue),
            double.MinValue,
            _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(double.MaxValue, unbounded.Result);
    }

    [Fact]
    public void HandlesInfinityAsTarget()
    {
        var result = TestReductionRunner.ForReducer(
            Reduce.Towards(double.PositiveInfinity),
            0d,
            _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(double.PositiveInfinity, unbounded.Result);
    }

    [Fact]
    public void TargetAsInitialValueIsUnbounded()
    {
        var result = TestReductionRunner.ForReducer(Reduce.Towards(12.34d), 12.34d, _ => true);

        var unbounded = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(12.34d, unbounded.Result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16)]
    public void RejectsInvalidPrecision(int precision)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Reduce.Towards(0d, precision));
    }

    [Fact]
    public void RejectsNaNTarget()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Reduce.Towards(double.NaN));
    }
}
