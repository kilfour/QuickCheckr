namespace QuickCheckr.UnderTheHood;

public static class Evaluate
{
    public static CheckrResult<Case> Expect(string key, Func<bool> condition, CheckrState state)
    {
        if (condition())
            state.EvidenceRecorder.ExpectationPassed(key);
        else
            state.EvidenceRecorder.ExpectationFailed(key);
        return CheckrResult.CaseOnly(state);
    }
}