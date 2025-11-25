using QuickCheckr.UnderTheHood;
using QuickPulse.Show;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Trace<T>(string key, T? obj) =>
        state =>
        {
            state.ShortTermMemories.StoreTrace(key, () => Introduce.This(obj!, false));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Trace<T>(string key, T? obj, Func<string, string> formatter) =>
        state =>
        {
            state.ShortTermMemories.StoreTrace(key, () => formatter(Introduce.This(obj!, false)));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Trace(string key, Func<string> trace) =>
        state =>
        {
            state.ShortTermMemories.StoreTrace(key, trace);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> TraceIf(string key, Func<bool> predicate, Func<string> trace) =>
        state => predicate() ? Trace(key, trace)(state) : CheckrResult.CaseOnly(state);
}