namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

/// <summary>
/// Produces the next reduction step for values of type <typeparamref name="T"/>.
/// </summary>
public delegate Reduction Reducer<T>(ReductionResult result);
