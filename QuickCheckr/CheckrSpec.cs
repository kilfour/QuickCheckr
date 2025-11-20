using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<Case> InnerSpec(this string key, Func<bool> condition) =>
        state =>
            {
                if (condition())
                    state.EvidenceRecorder.SpecPassed(key);
                else
                    state.EvidenceRecorder.SpecFailed(key);
                return CheckrResult.CheckOnly(state);
            };

    public static CheckrOf<Case> Spec(this string key, Func<bool> condition)
        => InnerSpec(key, condition);
}