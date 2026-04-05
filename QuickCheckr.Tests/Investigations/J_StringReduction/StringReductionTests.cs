using QuickCheckr.StringReduction;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class StringReductionTests : BaseTest
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;

        public void Warn(string key, Func<Warning> factory) { }
    }

    private readonly WhistleblowerSpy whistleblower = new();

    [Fact]
    public void Inside()
    {
        Func<Reducer<string>> reducerFactory = Select.Balanced('(', ')').Inside().Remove();
        var reducer = new TypeReducerOption<string>(reducerFactory).ReducerFactory();
        var result = ReductionRunner.ForReducer(reducer, "", "", "1 + ( 2 + 3)", a => true, whistleblower);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("1 + ()", step.Result);
    }
}
