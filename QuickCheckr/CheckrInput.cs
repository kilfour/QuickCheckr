using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public record InputCfg<T>(
        Func<T, IEnumerable<T>>? Reducer = null
    );

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<InputCfg<T>, InputCfg<T>> configure)
    {
        var cfg = configure(new InputCfg<T>());
        return state =>
        {
            if (cfg.Reducer != null)
                state.Memory.StoreReducer<object>(key, a => cfg.Reducer((T)a).Cast<object>());
            return HandleInput(state, key, fuzzr);
        };
    }

    /// <summary>
    /// Draws a named input value from a fuzzr and records it for reporting and shrinking.  
    /// Use when you want a straightforward generated input that participates in minimal failing case discovery.
    /// </summary>
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state => HandleInput(state, key, fuzzr);

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        try
        {
            state.Memory.StoreShrinkerForKey<T>(key);
            return CheckrResult.Some(state, state.Memory.StoreInput(key, () => fuzzr(state.FuzzrState).Value)!);
        }
        catch (DuplicateKeyWriteException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InputFuzzrThrewException(key, exception);
        }
    }

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => CheckrResult.Some(state, state.Memory.StoreInput(key, () => factory()(state.FuzzrState).Value)!);
}