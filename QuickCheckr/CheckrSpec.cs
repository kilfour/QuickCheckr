using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Spec(this string key, Func<bool> condition)
        => InnerSpec(key, condition);

    private static CheckrOf<Case> InnerSpec(this string key, Func<bool> condition) =>
        state =>
        {
            if (SpecNeedsToBeSkipped(key, state))
                return CheckrResult.CheckOnly(state);
            if (condition())
                state.EvidenceRecorder.SpecPassed(key);
            else
                state.EvidenceRecorder.SpecFailed(key);
            return CheckrResult.CheckOnly(state);
        };

    private static bool SpecNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && state.EvidenceRecorder.FailingSpec != key;
}