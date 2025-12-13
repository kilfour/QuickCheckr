using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<bool> condition)
        => state => state.IsThisTheRunsLastExecution() ? InnerSpec(key, condition)(state) : CheckrResult.CaseOnly(state);

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

