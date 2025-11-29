using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Perform(Action action) =>
        state => { action(); return CheckrResult.CaseOnly(state); };
    public static CheckrOf<Case> Perform(Task action) =>
        state => { action.GetAwaiter().GetResult(); return CheckrResult.CaseOnly(state); };
}