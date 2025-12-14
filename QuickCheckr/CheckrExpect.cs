using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named expectation that must evaluate to true for the case to pass.  
    /// Use to assert expected behaviour and have failures trigger shrinking and detailed reporting.
    /// </summary>
    public static CheckrOf<Case> Expect(string key, Func<bool> condition)
        => InnerExpect(key, condition);

    /// <summary>
    /// Adds a named conditional expectation that is only checked when the predicate holds.  
    /// Use when a spec is meaningful only in certain states and should be skipped otherwise.
    /// </summary>
    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition)
        => state => predicate() ? InnerExpect(key, condition)(state) : CheckrResult.CaseOnly(state);


    private static CheckrOf<Case> InnerExpect(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.ExpectGuard.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state,
                () => Evaluate.Expect(key, condition, state),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(ex));
        };
}
