using QuickCheckr.UnderTheHood.Docketing;

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
            state.Docket.Record(new ExpectationCaughtActionWasNotExecuted(key));
            return CheckrResult.Default<T>(state);
        }
        catch (Exception exception)
        {
            state.Docket.Record(new EvidenceFailure(exception));
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }
}