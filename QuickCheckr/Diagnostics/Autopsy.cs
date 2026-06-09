using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Diagnostics;

public static class Autopsy
{
    public static CheckrOf<Case> Note<T>(string key, Func<T?> factory) =>
        state =>
        {
            if (state.Pathologist.IsWatching)
                state.Pathologist.Inform("Note", key, state.Editor.IntroduceThis(factory()));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Note(string key, Func<string> trace) =>
        state =>
        {
            if (state.Pathologist.IsWatching)
                state.Pathologist.Inform("Note", key, trace());
            return CheckrResult.CaseOnly(state);
        };
}
