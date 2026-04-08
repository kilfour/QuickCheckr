using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowardsIntTests : BaseTest
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
        var result = ReductionRunner.ForReducer(new TypeReducerOption<int>(Reduce.Towards(0)).ReducerFactory(), "", "", 42, a => true, whistleblower);
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
        var result = ReductionRunner.ForReducer(new TypeReducerOption<int>(Reduce.Towards(0)).ReducerFactory(), "", "", 1, a => (int)a != 0, whistleblower);
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

    [Fact(Skip = "wip")]
    public void Oracle()
    {
        var checkr =
            from start in Checkr.Input("Start", Fuzzr.Int())
            from target in Checkr.Input("Target", Fuzzr.Int())
            from stop in Checkr.Input("Stop", Fuzzr.Int())
            from model in Checkr.Act("Model", () => RunModel(start, target, stop))
            from sut in Checkr.ActCarefully("Sut", () => RunSut(start, target, stop))
            from noExc in Checkr.When(() => !sut.Threw,
                from stopMatch in Checkr.Expect("Matches", () => Equals(model, sut.Value))
                from traceType in Checkr.Trace("Types", () => $"Model: {model.GetType().Name}, Sut: {sut.Value?.GetType().Name}")
                from trace in Checkr.Trace("Results", () => GetTrace(model, sut.Value!))
                select Case.Closed)
            select Case.Closed;
        // checkr.Run(100.Runs(), a => a with { FileAs = "ReduceTowardsInt" });
        checkr.Conduct(
            1000.Investigations(),
            10.Runs(),
            1.ExecutionsPerRun(),
            a => a with { FileAs = "ReduceTowardsInt" });
    }

    private static string GetTrace(Reduction model, Reduction sut) =>
        $"{GetReductionResult(model)} /= {GetReductionResult(sut)}";

    private static object GetReductionResult(Reduction reduction) =>
        reduction switch
        {
            Reduction.Stop stop => stop.Result,
            Reduction.Unbounded unbounded => unbounded.Result,
            _ => throw new InvalidOperationException()
        };

    private Reduction RunSut(int start, int target, int stop) =>
        ReductionRunner.ForReducer(
            new ReduceTowards<int>(target).Step, "", "",
                start, a => (int)a != stop, whistleblower);

    private Reduction RunModel(int start, int target, int stop) =>
        ReductionRunner.ForReducer(
            new TypeReducerOption<int>(Reduce.Towards(target)).ReducerFactory(), "", "",
                start, a => (int)a != stop, whistleblower);
}
