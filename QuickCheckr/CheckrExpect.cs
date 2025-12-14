using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named expectation that must evaluate to true for the case to pass.  
    /// Use to assert expected behaviour and have failures trigger shrinking and detailed reporting.
    /// </summary>
    public static CheckrOf<Case> Expect(string key, Func<bool> condition)
        => Evaluate.Expect(key, condition);

    /// <summary>
    /// Adds a named conditional expectation that is only checked when the predicate holds.  
    /// Use when a spec is meaningful only in certain states and should be skipped otherwise.
    /// </summary>
    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition)
        => state => predicate() ? Evaluate.Expect(key, condition)(state) : CheckrResult.CaseOnly(state);
}
