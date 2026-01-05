using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => CheckrResult.Some(state, GetValue(key, factory, state)!);

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory, Func<IShrinkOption> shrinkOption)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory, Func<IShrinkOption[]> options)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
        Func<ReductionResult, Reduction> reducer)
    {
        return state =>
        {
            state.Memory.StoreReducer(key, reducer);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
        Func<IShrinkOption> shrinkOption,
        Func<ReductionResult, Reduction> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            state.Memory.StoreReducer(key, reducer);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
        Func<IShrinkOption[]> options,
        Func<ReductionResult, Reduction> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            state.Memory.StoreReducer(key, reducer);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }
}