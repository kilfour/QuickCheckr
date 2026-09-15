using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>Awaits a shrinkable action using the run's cancellation token.</summary>
    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<CancellationToken, Task<T>> func)
        => TryShrinkableCatchAsync(key, func);

    /// <summary>Awaits a shrinkable action using the run's cancellation token.</summary>
    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Func<CancellationToken, Task> action)
        => TryShrinkableCatchAsync(key, async token => { await action(token).ConfigureAwait(false); return Case.Closed; });

    /// <summary>Awaits a shrinkable action, capturing non-cancellation exceptions.</summary>
    public static CheckrOf<ActResult<DelayedResult<T>>> ShrinkableActCarefully<T>(string key, Func<CancellationToken, Task<T>> func)
        => TryShrinkableCaptureAsync(key,
            async token => new DelayedResult<T>(await func(token).ConfigureAwait(false)),
            ex => new DelayedResult<T>(ex));

    /// <summary>Awaits a shrinkable action, capturing non-cancellation exceptions.</summary>
    public static CheckrOf<ActResult<DelayedResult>> ShrinkableActCarefully(string key, Func<CancellationToken, Task> action)
        => TryShrinkableCaptureAsync(key,
            async token => { await action(token).ConfigureAwait(false); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<T> func)
        => TryShrinkableCatch(key, func);

    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<Task<T>> func)
        => TryShrinkableCatchAsync(key, _ => func());

    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Action action)
        => TryShrinkableCatch(key, () => { action(); return Case.Closed; });

    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Func<Task> action)
        => TryShrinkableCatchAsync(key, async _ => { await action().ConfigureAwait(false); return Case.Closed; });

    private static CheckrOf<ActResult<T>> TryShrinkableCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (state.IsInitialRun)
                state.Memory.StoreShrinkableAction(key);
            if (ActionNeedsToBeSkipped(key, state))
            {
                state.Pathologist.Inform("Action", key, "SKIPPED");
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            }
            return TheCatcher.InTheCheckr(state,
                () => CheckrResult.Some(state, ActResult<T>.Executed(func())),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.Memory.GetActions().Contains(key);

    public static CheckrOf<ActResult<DelayedResult>> ShrinkableActCarefully(string key, Action action)
        => TryShrinkableCapture(key,
            () => { action(); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    public static CheckrOf<ActResult<DelayedResult>> ShrinkableActCarefully(string key, Func<Task> action)
        => TryShrinkableCaptureAsync(key,
            async _ => { await action().ConfigureAwait(false); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    public static CheckrOf<ActResult<DelayedResult<T>>> ShrinkableActCarefully<T>(string key, Func<T> func)
        => TryShrinkableCapture(key,
            () => new DelayedResult<T>(func()),
            ex => new DelayedResult<T>(ex));

    public static CheckrOf<ActResult<DelayedResult<T>>> ShrinkableActCarefully<T>(string key, Func<Task<T>> func)
        => TryShrinkableCaptureAsync(key,
            async _ => new DelayedResult<T>(await func().ConfigureAwait(false)),
            ex => new DelayedResult<T>(ex));

    private static CheckrOf<ActResult<T>> TryShrinkableCapture<T>(string key, Func<T> func, Func<Exception, T> onError) =>
        state =>
        {
            if (state.IsInitialRun)
                state.Memory.StoreShrinkableAction(key);
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            try
            {
                return CheckrResult.Some(state, ActResult<T>.Executed(func()));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return CheckrResult.Some(state, ActResult<T>.Executed(onError(ex)));
            }
        };
}

/// <summary>
/// Represents the outcome of an action that may be skipped while shrinking a case.
/// </summary>
public sealed class ActResult<T>
{
    private readonly bool notExecuted = false;
    public bool HasExecuted => !notExecuted;
    private readonly T? result;
    public T Result
    {
        get
        {
            if (notExecuted)
                throw new CheckrWasNotExecuted();
            return result!;
        }
    }
    private ActResult() { notExecuted = true; }
    private ActResult(T result) => this.result = result;
    internal static ActResult<T> NotExecuted => new() { };
    internal static ActResult<T> Executed(T value) => new(value) { };
}
