using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<TOuter>> Map<TOuter, TInner>(
        Func<Reducer<TInner>> reducerFactory,
        Func<TOuter, TInner> get,
        Func<TOuter, TInner, TOuter> set)
        => () =>
        {
            var reducer = reducerFactory();

            return result =>
            {
                var outer = result.ValueAs<TOuter>();
                var reduction = reducer(new ReductionResult(result.Failed, get(outer)));

                return reduction switch
                {
                    Reduction.Next next =>
                        new Reduction.Next(set(outer, (TInner)next.Candidate)!),
                    Reduction.Stop stop =>
                        new Reduction.Stop(set(outer, (TInner)stop.Result)!),
                    Reduction.Unbounded unbounded =>
                        new Reduction.Unbounded(set(outer, (TInner)unbounded.Result)!),
                    _ => throw new InvalidOperationException()
                };
            };
        };
}
