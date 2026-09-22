using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<bool> evaluation)
        => Expect(key, _ => evaluation() ? ExpectationResult.Passed() : ExpectationResult.Failed());

    public static CheckrOf<Case> Expect(this string key, Func<ExpectationContext, ExpectationResult> evaluation)
        => state =>
            state.IsThisTheRunsLastExecution()
            ? Evaluate.Expectation(key, evaluation, true)(state)
            : CheckrResult.CaseOnly(state);
}

