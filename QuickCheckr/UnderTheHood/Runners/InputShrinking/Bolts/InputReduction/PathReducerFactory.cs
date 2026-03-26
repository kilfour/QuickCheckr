namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public record PathReducerFactory(string Path, Type TargetType, Func<Func<ReductionResult, Reduction>> ReducerFactory);
