using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => CheckrResult.Some(state, GetValue(key, factory, state)!);

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory, Func<IClassificationOption> shrinkOption)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory, Func<IClassificationOption[]> options)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    // public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
    //     Func<Reducer<T>> reducer)
    // {
    //     return state =>
    //     {
    //         state.Memory.StoreReducer(key, reducer);
    //         return CheckrResult.Some(state, GetValue(key, factory, state)!);
    //     };
    // }

    // public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
    //     Func<IShrinkOption> shrinkOption,
    //     Func<Reducer<T>> reducer)
    // {
    //     return state =>
    //     {
    //         state.Memory.StoreShrinker(key, () => [shrinkOption()]);
    //         state.Memory.StoreReducer(key, reducer);
    //         return CheckrResult.Some(state, GetValue(key, factory, state)!);
    //     };
    // }

    // public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
    //     Func<IShrinkOption[]> options,
    //     Func<Reducer<T>> reducer)
    // {
    //     return state =>
    //     {
    //         state.Memory.StoreShrinker(key, options);
    //         state.Memory.StoreReducer(key, reducer);
    //         return CheckrResult.Some(state, GetValue(key, factory, state)!);
    //     };
    // }
}