using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Spec(string key, Func<bool> condition)
    {
        return InnerSpec(key, condition);
    }

    public static CheckrOf<Case> SpecIf(string key, Func<bool> predicate, Func<bool> condition)
        => state => predicate() ? InnerSpec(key, condition)(state) : CheckrResult.CaseOnly(state);

    private static CheckrOf<Case> InnerSpec(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.SpecGuard.Check(key);
            if (state.EvidenceRecorder.SpecNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            try
            {
                if (condition())
                    state.EvidenceRecorder.SpecPassed(key);
                else
                    state.EvidenceRecorder.SpecFailed(key);
                return CheckrResult.CaseOnly(state);
            }
            catch (Exception exception)
            {
                state.Docket.Record(new EvidenceException(exception));
                state.EvidenceRecorder.RecordFailure(exception);
                return CheckrResult.CaseOnly(state);
            }
        };
}