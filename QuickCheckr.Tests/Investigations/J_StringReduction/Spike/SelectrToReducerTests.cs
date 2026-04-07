using QuickCheckr.StringReduction.Nuts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Investigations.J_StringReduction.Spike;

public class SelectrToReducerTests
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;
        public void Warn(string key, Func<Warning> factory) { }
    }

    private readonly WhistleblowerSpy whistleblower = new();

    private Reduction Act(Func<Reducer<string>> reducerFactory, string input, Func<object?, bool>? failTest = null)
    {
        var reducer = new TypeReducerOption<string>(reducerFactory).ReducerFactory();
        var predicate = failTest ?? (a => true);
        var result = ReductionRunner.ForReducer(reducer, "", "", input, predicate, whistleblower);
        return result;
    }

    [Fact]
    public void FirstTry()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        var reducer = selectr.Reduce();
        var result = Act(reducer, "abc");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("c", step.Result);
    }

    [Fact]
    public void Trickier()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Text()
            from replace in Selectr.Once('c', 'd').Replace(t => a_b.ToUpper())
            select Selection.Defined;
        var reducer = selectr.Reduce();
        var result = Act(reducer, "--ac--bd--", a => ((string)a!).Contains("d"));
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal("--aA--bd--", step.Result);
    }
}