namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public record TypeReducerFactory(Type TargetType, Func<Func<ReductionResult, Reduction>> ReducerFactory);

public interface ITypeReducerOption : IReducerOption
{
    Type TargetType { get; }
}

public record TypeReducerOption<T>(Func<Reducer<T>> Factory) : ITypeReducerOption
{
    public Type TargetType => typeof(T);
    public Func<Func<ReductionResult, Reduction>> ReducerFactory => () =>
    {
        var reducer = Factory();
        return a => reducer(a);
    };
}
