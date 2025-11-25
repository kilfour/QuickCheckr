using QuickCheckr.UnderTheHood;
using QuickPulse.Show;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Trace<T>(string key, Func<T?> factory) =>
        state =>
        {
            state.Memory.StoreTrace(key, () => Introduce.This(factory()!, false));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Trace<T>(string key, T? obj, Func<string, string> formatter) =>
        state =>
        {
            state.Memory.StoreTrace(key, () => formatter(Introduce.This(obj!, false)));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Trace(string key, Func<string> trace) =>
        state =>
        {
            state.Memory.StoreTrace(key, trace);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> TraceIf(string key, Func<bool> predicate, Func<string> trace) =>
        state => predicate() ? Trace(key, trace)(state) : CheckrResult.CaseOnly(state);
}