using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named conditional expectation that is only checked when the predicate holds.  
    /// Use when an expectation is meaningful only in certain states and should be skipped otherwise.
    /// </summary>
    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<bool> predicate,
        Func<ExpectationContext, ExpectationResult> evaluation)
        => Evaluate.ExpectationWhen(key, predicate, evaluation, true);

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> evaluation)
        => ExpectWhen(key, predicate,
            context => evaluation() ? context.Pass() : context.Fail());
}
