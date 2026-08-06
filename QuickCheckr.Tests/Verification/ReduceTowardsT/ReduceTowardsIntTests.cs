using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;

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
    public void ToZero()
    {
        var result = ReductionRunner.ForReducer(new ReduceTowards<int>(0).Step, "", "", 42, a => true, whistleblower);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0, step.Result);
    }

    [Fact]
    public void ToZeroOrg()
    {
        var result = TestReductionRunner.ForReducer(Reduce.Towards(0), 42, a => true);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(0, step.Result);
    }

    [Fact]
    public void ToZeroStopped()
    {
        var result = ReductionRunner.ForReducer(new ReduceTowards<int>(0).Step, "", "", 1, a => (int)a != 0, whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(1, step.Result);
    }

    [Fact]
    public void ToZeroStoppedOrg()
    {
        var result = TestReductionRunner.ForReducer(Reduce.Towards(0), 1, a => (int)a != 0, whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(1, step.Result);
    }

    [Fact]
    public void GoingUp()
    {
        var result = ReductionRunner.ForReducer(new ReduceTowards<int>(100).Step, "", "", 1, a => (int)a < 90, whistleblower);
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal(89, step.Result);
    }

    [Fact]
    public void Checking()
    {
        var result = ReductionRunner.ForReducer(new ReduceTowards<int>(5).Step, "", "", 72, a => (int)a >= 2, whistleblower);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal(5, step.Result);
    }
}
