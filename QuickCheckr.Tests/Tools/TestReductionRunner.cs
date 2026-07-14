using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Tools;

public class TestReductionRunner
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;

        public void Warn(string key, Func<Warning> factory) { }
    }

    public static Reduction ForReducer<T>(
        Func<Reducer<T>> reducerFactory,
        T? value,
        Func<object, bool> failTest,
        IReductionWhistleblower? whistleblower = null)
    {
        var reducer = reducerFactory();

        return ReductionRunner.ForReducer(
            result => reducer(result),
            string.Empty,
            string.Empty,
            value,
            failTest,
            whistleblower ?? new WhistleblowerSpy());
    }
}