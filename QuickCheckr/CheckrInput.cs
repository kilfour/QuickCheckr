using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public record InputCfg<T>(
        Func<T, IEnumerable<T>>? Shrinker = null,
        int FuzzrShrinker = 0,
        Func<T, IEnumerable<T>>? Reducer = null
    );

    /// <summary>
    /// Draws a named input value from a fuzzr and optionally configures how it will be shrunk or reduced.  
    /// Use when you want to attach custom shrinkers or reducers to an input so QuickCheckr can guide shrinking more precisely.
    /// </summary>
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<InputCfg<T>, InputCfg<T>> configure)
    {
        var cfg = configure(new InputCfg<T>());
        return state =>
        {
            if (cfg.FuzzrShrinker > 0)
            {
                state.Memory.StoreShrinker<object>(key,
                    a => fuzzr.Unique($"{key}").Many(cfg.FuzzrShrinker)(state.FuzzrState).Value.Cast<object>());
            }
            if (cfg.Shrinker != null)
                state.Memory.StoreShrinker<object>(key, a => cfg.Shrinker((T)a).Cast<object>());
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

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => HandleInput(state, key, factory());


    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
        => CheckrResult.Some(state,
            state.Memory.StoreInput(key, () => fuzzr(state.FuzzrState).Value)!);
}