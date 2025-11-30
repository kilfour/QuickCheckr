using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Act<T>(string key, Func<T> func)
        => TryCatch(key, func);
    public static CheckrOf<T> Act<T>(string key, Task<T> func)
        => TryCatch(key, () => func.GetAwaiter().GetResult());

    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });
    public static CheckrOf<Case> Act(string key, Task action)
        => TryCatch(key, () => { action.GetAwaiter().GetResult(); return Case.Closed; });


    public static CheckrOf<T> ActIf<T>(this string key, Func<bool> predicate, Func<T> func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.Default<T>(state);
    public static CheckrOf<T> ActIf<T>(this string key, Func<bool> predicate, Task<T> func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.Default<T>(state);

    public static CheckrOf<Case> ActIf(this string key, Func<bool> predicate, Action func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);
    public static CheckrOf<Case> ActIf(this string key, Func<bool> predicate, Task func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);

    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Default<T>(state);
            state.Memory.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception exception)
            {
                state.Docket.Record(new EvidenceFailure(exception));
                state.EvidenceRecorder.RecordFailure(exception);
                return CheckrResult.Default<T>(state);
            }
        };

    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.Memory.HasAction(key);
}