using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;


public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state => HandleInput(state, key, fuzzr);

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<IShrinkOption> shrinkOption)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<IShrinkOption[]> options)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            return HandleInput(state, key, fuzzr);
        };
    }


    public static CheckrOf<T> Input<T, Y>(string key, FuzzrOf<T> fuzzr, Func<Reducer<Y>> reducer)
    {
        return state =>
        {
            state.Memory.StoreReducerByType(key, reducer);
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T, Y>(string key, FuzzrOf<T> fuzzr, (string, Func<Reducer<Y>>)[] pathReducers)
    {
        return state =>
        {
            state.Memory.StoreReducerByPath(key, pathReducers);
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T, Y>(string key, FuzzrOf<T> fuzzr,
        Func<IShrinkOption> shrinkOption,
        Func<Reducer<Y>> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            state.Memory.StoreReducerByType(key, reducer);
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T, Y>(string key, FuzzrOf<T> fuzzr,
        Func<IShrinkOption[]> options,
        Func<Reducer<Y>> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            state.Memory.StoreReducerByType(key, reducer);
            return HandleInput(state, key, fuzzr);
        };
    }

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        try { return CheckrResult.Some(state, GetValue(key, () => fuzzr, state)!); }
        catch (DuplicateKeyWriteException) { throw; }
        catch (Exception exception) { throw new InputFuzzrThrewException(key, exception); }
    }

    private static T? GetValue<T>(string key, Func<FuzzrOf<T>> factory, CheckrState state)
        => state.Memory.StoreInput(key, () => factory()(state.FuzzrState).Value);
}