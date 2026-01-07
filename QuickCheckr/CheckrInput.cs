using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
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

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Reducer<T> reducer)
    {
        return state =>
        {
            state.Memory.StoreReducer(key, a => reducer(a));
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr,
        Func<IShrinkOption> shrinkOption,
        Reducer<T> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, () => [shrinkOption()]);
            state.Memory.StoreReducer(key, a => reducer(a));
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr,
        Func<IShrinkOption[]> options,
        Reducer<T> reducer)
    {
        return state =>
        {
            state.Memory.StoreShrinker(key, options);
            state.Memory.StoreReducer(key, a => reducer(a));
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