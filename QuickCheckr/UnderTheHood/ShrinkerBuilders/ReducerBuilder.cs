using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public class ReducerBuilder<T>
{
    public Func<Reducer<T>>[] Simplify(params Func<Reducer<T>>[] reducers)
        => reducers;
}