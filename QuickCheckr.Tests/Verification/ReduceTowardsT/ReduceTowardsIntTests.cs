using QuickCheckr.Shrinking;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowardsIntTests
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;

        public void Warn(string key, Func<Warning> factory) { }
    }

    private readonly WhistleblowerSpy whistleblower = new();

    [Fact]
    public async Task ToZero()
    {
        var result = await ReductionRunner.ForReducerAsync(new ReduceTowards<int>(0).Step, "", "", 42, a => new ValueTask<bool>(true), whistleblower);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0, step.Result);
    }

    [Fact]
    public async Task ToZeroOrg()
    {
        var result = await TestReductionRunner.ForReducerAsync(Reduce.Towards(0), 42, a => true);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0, step.Result);
    }

    [Fact]
    public async Task ToZeroStopped()
    {
        var result = await ReductionRunner.ForReducerAsync(new ReduceTowards<int>(0).Step, "", "", 1, a => new ValueTask<bool>((int)a != 0), whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(1, step.Result);
    }

    [Fact]
    public async Task ToZeroStoppedOrg()
    {
        var result = await TestReductionRunner.ForReducerAsync(Reduce.Towards(0), 1, a => (int)a != 0, whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(1, step.Result);
    }

    [Fact]
    public async Task GoingUp()
    {
        var result = await ReductionRunner.ForReducerAsync(new ReduceTowards<int>(100).Step, "", "", 1, a => new ValueTask<bool>((int)a < 90), whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89, step.Result);
    }

    [Fact]
    public async Task Checking()
    {
        var result = await ReductionRunner.ForReducerAsync(new ReduceTowards<int>(5).Step, "", "", 72, a => new ValueTask<bool>((int)a >= 2), whistleblower);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(5, step.Result);
    }
}
