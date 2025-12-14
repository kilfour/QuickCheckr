using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<bool> condition)
        => state => state.IsThisTheRunsLastExecution() ? InnerSpec(key, condition)(state) : CheckrResult.CaseOnly(state);

    private static CheckrOf<Case> InnerSpec(string key, Func<bool> condition) =>
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

