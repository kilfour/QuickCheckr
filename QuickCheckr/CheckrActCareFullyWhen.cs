using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<DelayedResult<T>> ActCarefullyWhen<T>(string key, Func<bool> predicate, Func<T> func)
        => state => predicate() ? ActCarefully(key, func)(state) : CheckrResult.Some(state, DelayedResult<T>.None());
}