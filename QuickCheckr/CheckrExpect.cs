using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named specification that must evaluate to true for the case to pass.  
    /// Use to assert expected behaviour and have failures trigger shrinking and detailed reporting.
    /// </summary>
    public static CheckrOf<Case> Expect(string key, Func<bool> condition)
        => InnerSpec(key, condition);

    /// <summary>
    /// Adds a named conditional specification that is only checked when the predicate holds.  
    /// Use when a spec is meaningful only in certain states and should be skipped otherwise.
    /// </summary>
    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition)
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
                state.Docket.Record(new EvidenceFailure(exception));
                state.EvidenceRecorder.SpecCrashed(exception);
                return CheckrResult.CaseOnly(state);
            }
        };
}