using QuickCheckr.Shrinking;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceMinimizeString;

public class CanHandleNull
{
    [Fact]
    public async Task NullIsAlreadyMinimal()
    {
        var verifierWasCalled = false;
        var result = await TestReductionRunner.ForReducerAsync(
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
