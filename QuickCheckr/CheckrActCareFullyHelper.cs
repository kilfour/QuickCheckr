using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<T> TryCapture<T>(this string key, Func<T> func, Func<Exception, T> onError) =>
        state =>
        {
            state.Memory.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return CheckrResult.Some(state, onError(ex));
            }
        };
}