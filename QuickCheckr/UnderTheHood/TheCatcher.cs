namespace QuickCheckr.UnderTheHood;

public static class TheCatcher
{
    public static CheckrResult<T> InTheCheckr<T>(
        CheckrState state,
        string key,
        Func<CheckrResult<T>> func,
        Action<Exception>? onException = null)
    {
        try
        {
            return func();
        }
        catch (CheckrWasNotExecuted)
        {
            return CheckrResult.Default<T>(state);
        }
        catch (Exception exception)
        {
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }
}