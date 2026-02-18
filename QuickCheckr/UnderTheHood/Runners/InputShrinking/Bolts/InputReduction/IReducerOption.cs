namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public interface IReducerOption
{
    Func<Func<ReductionResult, Reduction>> ReducerFactory { get; }
};
