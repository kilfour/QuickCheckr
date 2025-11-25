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

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<InputCfg<T>, InputCfg<T>> configure)
    {
        var cfg = configure(new InputCfg<T>());
        return state =>
        {
            if (cfg.FuzzrShrinker > 0)
            {
                var executionId = state.Memory.ShortTermMemory.CurrentExecutionNumber; // TODO: this is a hack, FIXME
                state.Memory.StoreShrinker<object>(key,
                    a => fuzzr.Unique($"{executionId}-{key}").Many(cfg.FuzzrShrinker)(state.FuzzrState).Value.Cast<object>());
            }
            if (cfg.Shrinker != null)
                state.Memory.StoreShrinker<object>(key, a => cfg.Shrinker((T)a).Cast<object>());
            if (cfg.Reducer != null)
                state.Memory.StoreReducer<object>(key, a => cfg.Reducer((T)a).Cast<object>());
            return HandleInput(state, key, fuzzr);
        };
    }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state => HandleInput(state, key, fuzzr);

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
        => CheckrResult.Some(state,
            state.Memory.StoreInput(key, () => fuzzr(state.FuzzrState).Value)!);
}