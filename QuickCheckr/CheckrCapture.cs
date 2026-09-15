using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Captures the value produced by a function without adding an action to the report.  
    /// Use when you want to compute state or helper values that participate in the script but do not represent observable behaviour.
    /// </summary>
    public static CheckrOf<T> Capture<T>(Func<T> func) =>
        state => TheCatcher.InTheCheckr(state,
            () => CheckrResult.Some(state, func()),
            ex => state.EvidenceRecorder.RecordFailure(ex));


    /// <summary>
    /// Captures the value produced by an asynchronous function without adding an action to the report.  
    /// Use when awaiting a helper computation that should influence the script but not appear as an action.
    /// </summary>
    public static CheckrOf<T> Capture<T>(Func<Task<T>> func) =>
        Capture(_ => func());

    /// <summary>Captures an asynchronous value using the run's cancellation token.</summary>
    public static CheckrOf<T> Capture<T>(Func<CancellationToken, Task<T>> func) =>
        state => TheCatcher.InTheCheckrAsync(state,
            async () => CheckrResult.Some(state, await func(state.CancellationToken).ConfigureAwait(false)),
            ex => state.EvidenceRecorder.RecordFailure(ex));
}
