namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

internal interface ITypeReducerOption : IReducerOption
{
    Type TargetType { get; }
}

internal sealed record TypeReducerOption<T>(Func<Reducer<T>> Factory) : ITypeReducerOption
{
    public Type TargetType => typeof(T);
    public Func<Func<ReductionResult, Reduction>> ReducerFactory => () =>
    {
        var reducer = Factory();
        return a => reducer(a);
    };
}
