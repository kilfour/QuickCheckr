using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<bool> condition)
        => state => state.IsThisTheRunsLastExecution() ? Evaluate.Expect(key, condition)(state) : CheckrResult.CaseOnly(state);
}

