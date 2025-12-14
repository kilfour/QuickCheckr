using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Marks a named condition as proven when it first evaluates to true and stops the current run early.  
    /// Use for properties that require only one successful observation instead of completing all remaining executions.
    /// </summary>
    public static CheckrOf<Case> StopWhenProven(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.ExpectGuard.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            try
            {
                if (condition())
                {
                    state.EvidenceRecorder.ExpectationPassed(key);
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