using QuickCheckr.Shrinking;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory) =>
        state => CheckrResult.Some(state, GetValue(key, factory, state)!);

    public static CheckrOf<T> Input<T>(string key, Func<FuzzrOf<T>> factory, params Shrinker[] shrinkers)
    {
        return state =>
        {
            state.Memory.StoreShrinkers(key, shrinkers);
            return CheckrResult.Some(state, GetValue(key, factory, state)!);
        };
    }
}