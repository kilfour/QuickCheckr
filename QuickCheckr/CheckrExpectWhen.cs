using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named conditional expectation that is only checked when the predicate holds.  
    /// Use when an expectation is meaningful only in certain states and should be skipped otherwise.
    /// </summary>
    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition)
        => InnerExpectWhen(key, predicate, condition, () => []);

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition, Func<string> message)
        => InnerExpectWhen(key, predicate, condition, () => [message()]);

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<bool> condition, Func<List<string>>? messages)
        => InnerExpectWhen(key, predicate, condition, messages);

    private static CheckrOf<Case> InnerExpectWhen(string key, Func<bool> predicate, Func<bool> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, messages,
            _ => Task.FromResult(predicate()));
}