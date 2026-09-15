using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<T> TryCatchAsync<T>(string key, Func<CancellationToken, Task<T>> func) =>
        state =>
        {
            if (state.IsInitialRun)
                state.Memory.StoreAction(key);
            return TheCatcher.InTheCheckrAsync(state,
                async () => CheckrResult.Some(state, await func(state.CancellationToken).ConfigureAwait(false)),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    private static CheckrOf<T> TryCaptureAsync<T>(string key, Func<CancellationToken, Task<T>> func, Func<Exception, T> onError) =>
        async state =>
        {
            state.CancellationToken.ThrowIfCancellationRequested();
            state.Memory.StoreAction(key);
            try
            {
                var result = await func(state.CancellationToken).ConfigureAwait(false);
                state.CancellationToken.ThrowIfCancellationRequested();
                return CheckrResult.Some(state, result);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { return CheckrResult.Some(state, onError(ex)); }
        };

    private static CheckrOf<ActResult<T>> TryShrinkableCatchAsync<T>(string key, Func<CancellationToken, Task<T>> func) =>
        state =>
        {
            if (state.IsInitialRun)
                state.Memory.StoreShrinkableAction(key);
            if (ActionNeedsToBeSkipped(key, state))
            {
                state.Pathologist.Inform("Action", key, "SKIPPED");
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            }
            return TheCatcher.InTheCheckrAsync(state,
                async () => CheckrResult.Some(state, ActResult<T>.Executed(await func(state.CancellationToken).ConfigureAwait(false))),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    private static CheckrOf<ActResult<T>> TryShrinkableCaptureAsync<T>(string key, Func<CancellationToken, Task<T>> func, Func<Exception, T> onError) =>
        async state =>
        {
            state.CancellationToken.ThrowIfCancellationRequested();
            if (state.IsInitialRun)
                state.Memory.StoreShrinkableAction(key);
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            try
            {
                var result = await func(state.CancellationToken).ConfigureAwait(false);
                state.CancellationToken.ThrowIfCancellationRequested();
                return CheckrResult.Some(state, ActResult<T>.Executed(result));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { return CheckrResult.Some(state, ActResult<T>.Executed(onError(ex))); }
        };
}
