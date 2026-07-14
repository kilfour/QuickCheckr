using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Verification.ReduceMinimizeString;

public class CanHandleNull
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;

        public void Warn(string key, Func<Warning> factory) { }
    }

    private readonly WhistleblowerSpy whistleblower = new();

    [Fact]
    public void NullIsAlreadyMinimal()
    {
        var verifierWasCalled = false;
        var reducer = new TypeReducerOption<string>(Reduce.MinimizeString).ReducerFactory();

        var result = ReductionRunner.ForReducer(
            reducer,
            "",
            "",
            null,
            _ =>
            {
                verifierWasCalled = true;
                return true;
            },
            whistleblower);

        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Null(stop.Result);
        Assert.False(verifierWasCalled);
    }
}
