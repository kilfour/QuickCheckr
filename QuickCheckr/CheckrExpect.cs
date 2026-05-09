using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named expectation that must evaluate to true for the case to pass.  
    /// Use to assert expected behaviour and have failures trigger shrinking and detailed reporting.
    /// </summary>
    public static CheckrOf<Case> Expect(string key, Func<bool> condition)
        => InnerExpect(key, condition, () => []);

    public static CheckrOf<Case> Expect(string key, Func<bool> condition, Func<string> message)
        => InnerExpect(key, condition, () => [message()]);

    public static CheckrOf<Case> Expect(string key, Func<bool> condition, Func<List<string>>? messages)
        => InnerExpect(key, condition, messages);

    private static CheckrOf<Case> InnerExpect(string key, Func<bool> condition, Func<List<string>>? messages)
        => Evaluate.Expect(key, condition, true, messages);
}
