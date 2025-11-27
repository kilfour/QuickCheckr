using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> ProvenWhen(string key, Func<bool> condition) =>
        state =>
        {
            if (SpecNeedsToBeSkipped(key, state))
                return CheckrResult.CaseOnly(state);
            try
            {
                if (condition())
                {
                    state.EvidenceRecorder.SpecPassed(key);
                    state.StopRun(); // <= TODO: Everything except this line is duplication from CheckrSpec 
                }                    //    And the fact there's no SpecFailed(key)
                return CheckrResult.CaseOnly(state);
            }
            catch (Exception exception)
            {
                state.Docket.Record(new EvidenceException(exception));
                state.EvidenceRecorder.RecordFailure(exception);
                return CheckrResult.CaseOnly(state);
            }
        };

    // TODO: Duplication: See CheckrSpec
    private static bool SpecNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && state.EvidenceRecorder.FailingSpec != key;
}