namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

internal interface IReducerOption
{
    Func<Func<ReductionResult, Reduction>> ReducerFactory { get; }
};
