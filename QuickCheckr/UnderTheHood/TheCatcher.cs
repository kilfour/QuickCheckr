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
            state.CancellationToken.ThrowIfCancellationRequested();
            return func();
        }
        catch (CheckrWasNotExecuted)
        {
            state.Pathologist.Inform("CheckrWasNotExecuted");
            return CheckrResult.Default<T>(state);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }

    public static async ValueTask<CheckrResult<T>> InTheCheckrAsync<T>(
        CheckrState state,
        Func<ValueTask<CheckrResult<T>>> func,
        Action<Exception>? onException = null)
    {
        try
        {
            state.CancellationToken.ThrowIfCancellationRequested();
            var result = await func().ConfigureAwait(false);
            state.CancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (CheckrWasNotExecuted)
        {
            state.Pathologist.Inform("CheckrWasNotExecuted");
            return CheckrResult.Default<T>(state);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }
}
