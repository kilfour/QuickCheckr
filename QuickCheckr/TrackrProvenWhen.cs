using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> ProvenWhen(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.SpecGuard.Check(key);
            if (state.EvidenceRecorder.SpecNeedsToBeSkipped(key))
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
                state.Docket.Record(new EvidenceFailure(exception));
                state.EvidenceRecorder.RecordFailure(exception);
                return CheckrResult.CaseOnly(state);
            }
        };
}