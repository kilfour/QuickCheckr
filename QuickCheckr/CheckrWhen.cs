using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Executes the given Checkr step only when the predicate evaluates to true.  
    /// Use to conditionally include parts of a script without affecting the surrounding case when skipped.
    /// </summary>
    public static CheckrOf<ConditionalResult<T>> When<T>(Func<bool> predicate, CheckrOf<T> checkr) =>
        state => ConditionalResult<T>.AsCheckrResult(state, predicate, () => checkr(state).Value);
}