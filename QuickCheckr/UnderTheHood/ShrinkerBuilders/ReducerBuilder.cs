using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

/// <summary>
/// Selects the reducers used to simplify inputs of type <typeparamref name="T"/>.
/// </summary>
public sealed class ReducerBuilder<T>
{
    public Func<Reducer<T>>[] Simplify(params Func<Reducer<T>>[] reducers)
        => reducers;

    public Func<Reducer<string>>[] Simplify(params SelectrOf<Selection>[] selectors)
        => selectors.Select(a => a.Reduce()).ToArray();
}
