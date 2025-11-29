using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Execute(Action action) =>
        state => { action(); return CheckrResult.CaseOnly(state); };
    public static CheckrOf<Case> Execute(Task action) =>
        state => { action.GetAwaiter().GetResult(); return CheckrResult.CaseOnly(state); };

    public static CheckrOf<T> Execute<T>(Func<T> func) =>
        state => CheckrResult.Some(state, func());
    public static CheckrOf<T> Execute<T>(Task<T> func) =>
        state => CheckrResult.Some(state, func.GetAwaiter().GetResult());
}