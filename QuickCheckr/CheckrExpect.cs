using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Expect(string key, Func<ExpectationContext, ExpectationResult> evaluation)
        => Evaluate.Expectation(key, evaluation, true);

    public static CheckrOf<Case> Expect(string key, Func<bool> evaluation)
        => Expect(key, _ => evaluation() ? ExpectationResult.Passed() : ExpectationResult.Failed());
}
