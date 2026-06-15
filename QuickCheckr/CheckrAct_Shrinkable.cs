using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<T> func)
        => TryShrinkableCatch(key, func);

    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<Task<T>> func)
        => TryShrinkableCatch(key, () => func().GetAwaiter().GetResult());

    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Action action)
        => TryShrinkableCatch(key, () => { action(); return Case.Closed; });

    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Func<Task> action)
        => TryShrinkableCatch(key, () => { action().GetAwaiter().GetResult(); return Case.Closed; });

    private static CheckrOf<ActResult<T>> TryShrinkableCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (!state.EvidenceRecorder.IsReplay) // HACK
                state.Memory.StoreAction(key);
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

    public static CheckrOf<ActResult<DelayedResult>> ShrinkableActCarefully(string key, Task action)
        => TryShrinkableCapture(key,
            () => { action.GetAwaiter().GetResult(); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    public static CheckrOf<ActResult<DelayedResult<T>>> ShrinkableActCarefully<T>(string key, Func<T> func)
        => TryShrinkableCapture(key,
            () => new DelayedResult<T>(func()),
            ex => new DelayedResult<T>(ex));

    public static CheckrOf<ActResult<DelayedResult<T>>> ShrinkableActCarefully<T>(string key, Task<T> func)
        => TryShrinkableCapture(key,
            () => new DelayedResult<T>(func.GetAwaiter().GetResult()),
            ex => new DelayedResult<T>(ex));

    private static CheckrOf<ActResult<T>> TryShrinkableCapture<T>(string key, Func<T> func, Func<Exception, T> onError) =>
        state =>
        {
            if (!state.EvidenceRecorder.IsReplay) // HACK
                state.Memory.StoreAction(key);
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            try
            {
                return CheckrResult.Some(state, ActResult<T>.Executed(func()));
            }
            catch (Exception ex)
            {
                return CheckrResult.Some(state, ActResult<T>.Executed(onError(ex)));
            }
        };
}

public class ActResult<T>
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
    public static ActResult<T> NotExecuted => new() { };
    public static ActResult<T> Executed(T value) => new(value) { };
}
