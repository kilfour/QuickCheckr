using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood;

public static class TheCatcher
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
        catch (ActionResultIsNone)
        {
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