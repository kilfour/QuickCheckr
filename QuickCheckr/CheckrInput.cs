using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;



public static partial class Checkr
{
    // public class InputCfg<T>
    // {
    //     public Func<T, string>? Formatter { get; private set; }
    //     public InputCfg<T> Trace() { Formatter = a => Introduce.This(a!, false); return this; }
    //     public InputCfg<T> Trace(Func<T, string> formatter) { Formatter = formatter; return this; }
    // }

    // public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, Func<InputCfg<T>, InputCfg<T>> configure)
    // {
    //     var cfg = configure(new InputCfg<T>());
    //     return state =>
    //     {
    //         return HandleInput(state, key, fuzzr, cfg);
    //     };
    // }

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr)
    {
        //var cfg = new InputCfg<T>();
        return state =>
        {
            return HandleInput(state, key, fuzzr);
        };
    }

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        var value = state.ShortTermMemories.Current.StoreInput(key, () => fuzzr(state.FuzzrState).Value);
        return CheckrResult.Some(state, value!);
    }
}