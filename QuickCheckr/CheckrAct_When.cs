using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{

    /// <summary>
    /// Adds a named action step that only executes when a condition holds, capturing its result for later expectations and traces.
    /// Use when a synchronous operation is optional but should be reported whenever it runs.
    /// </summary>
    public static CheckrOf<ConditionalResult<T>> ActWhen<T>(this string key, Func<bool> predicate, Func<T> func) =>
        When(predicate, Act(key, func));

    /// <summary>
    /// Adds a named asynchronous action step that only executes when a condition holds, capturing its result for later expectations and traces.
    /// Use when an async operation is optional but should still appear as a proper step whenever it runs.
    /// </summary>s
    public static CheckrOf<ConditionalResult<T>> ActWhen<T>(this string key, Func<bool> predicate, Func<Task<T>> func) =>
        When(predicate, Act(key, func));

    /// <summary>
    /// Adds a named action step that only executes when a condition holds, for side-effect-only operations.
    /// Use when the operation is optional and you simply want the case to continue unchanged when it is skipped.
    /// </summary>
    public static CheckrOf<Case> ActWhen(this string key, Func<bool> predicate, Action func) =>
        state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);

    /// <summary>
    /// Adds a named asynchronous action step that only executes when a condition holds, for side-effect-only operations.
    /// Use when an async operation is optional and the case should continue unchanged when it is skipped.
    /// </summary>
    public static CheckrOf<Case> ActWhen(this string key, Func<bool> predicate, Func<Task> func) =>
        state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);
}
