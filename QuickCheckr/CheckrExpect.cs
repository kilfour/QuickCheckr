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
}
