using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state => HandleInput(state, key, fuzzr);

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, params Shrinker[] shrinkers)
    {
        return state =>
        {
            state.Memory.StoreShrinkers(key, shrinkers);
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