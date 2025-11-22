using QuickCheckr.UnderTheHood;
using QuickPulse.Show;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Trace<T>(this string key, T? obj) =>
        state =>
        {
            state.ShortTermMemories.Current.StoreTrace(key, () => Introduce.This(obj!, false));
            return CheckrResult.CheckOnly(state);
        };

    public static CheckrOf<Case> Trace<T>(this string key, T? obj, Func<string, string> formatter) =>
        state =>
        {
            state.ShortTermMemories.Current.StoreTrace(key, () => formatter(Introduce.This(obj!, false)));
            return CheckrResult.CheckOnly(state);
        };

    public static CheckrOf<Case> Trace(this string key, Func<string> trace) =>
        state =>
        {
            state.ShortTermMemories.Current.StoreTrace(key, trace);
            return CheckrResult.CheckOnly(state);
        };

    public static CheckrOf<Case> TraceIf(this string key, Func<bool> predicate, Func<string> trace) =>
        state => predicate() ? key.Trace(trace)(state) : CheckrResult.CheckOnly(state);
}