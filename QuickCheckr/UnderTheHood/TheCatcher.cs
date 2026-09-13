namespace QuickCheckr.UnderTheHood;

internal static class TheCatcher
{
    public static CheckrResult<T> InTheCheckr<T>(
        CheckrState state,
        Func<CheckrResult<T>> func,
        Action<Exception>? onException = null)
    {
        try
        {
            return func();
        }
        catch (CheckrWasNotExecuted)
        {
            state.Pathologist.Inform("CheckrWasNotExecuted");
            return CheckrResult.Default<T>(state);
        }
        catch (Exception exception)
        {
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }
}
