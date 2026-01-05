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

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, params ShrinkOption<T>[] options)
    {
        return state =>
        {
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr,
        Func<ReductionResult, Reduction> reducer)
    {
        return state =>
        {
            state.Memory.StoreReducer(key, reducer);
            return HandleInput(state, key, fuzzr);
        };
    }

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        try { return CheckrResult.Some(state, GetValue(key, () => fuzzr, state)!); }
        catch (DuplicateKeyWriteException) { throw; }
        catch (Exception exception) { throw new InputFuzzrThrewException(key, exception); }
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => CheckrResult.Some(state, GetValue(key, factory, state)!);

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory,
        Func<ReductionResult, Reduction> reducer)
    {
        return state =>
        {
            state.Memory.StoreReducer(key, reducer);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }

    private static T? GetValue<T>(string key, Func<FuzzrOf<T>> factory, CheckrState state)
        => state.Memory.StoreInput(key, () => factory()(state.FuzzrState).Value);
}