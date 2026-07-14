using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceMinimizeString;

public class CanHandleNull
{
    [Fact]
    public void NullIsAlreadyMinimal()
    {
        var verifierWasCalled = false;
        var result = TestReductionRunner.ForReducer(
            Reduce.MinimizeString,
            null,
            _ =>
            {
                verifierWasCalled = true;
                return true;
            });
        var stop = Assert.IsType<Reduction.Stop>(result);
        Assert.Null(stop.Result);
        Assert.False(verifierWasCalled);
    }
}
