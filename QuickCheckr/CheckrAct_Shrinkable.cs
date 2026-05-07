using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<ActResult<T>> ShrinkableAct<T>(string key, Func<T> func)
        => TryShrinkableCatch(key, func);

    // public static CheckrOf<T> ShrinkableAct<T>(string key, Func<Task<T>> func)
    //     => TryShrinkableCatch(key, () => func().GetAwaiter().GetResult());

    public static CheckrOf<ActResult<Case>> ShrinkableAct(string key, Action action)
        => TryShrinkableCatch(key, () => { action(); return Case.Closed; });

    // public static CheckrOf<Case> ShrinkableAct(string key, Func<Task> action)
    //     => TryShrinkableCatch(key, () => { action().GetAwaiter().GetResult(); return Case.Closed; });

    private static CheckrOf<ActResult<T>> TryShrinkableCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Some(state, ActResult<T>.NotExecuted);
            if (!state.EvidenceRecorder.IsReplay) // HACK
                state.Memory.StoreAction(key);
            return TheCatcher.InTheCheckr(state,
                () => CheckrResult.Some(state, ActResult<T>.Executed(func())),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };
    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.Memory.GetActions().Contains(key);
}

public class ActResult<T>
{
    private readonly bool notExecuted = false;
    private readonly T? value;
    public T? Value
    {
        get
        {
            if (notExecuted)
                throw new CheckrWasNotExecuted();
            return value;
        }
    }
    private ActResult() { notExecuted = true; }
    private ActResult(T? value) => this.value = value;
    public static ActResult<T> NotExecuted => new() { };
    public static ActResult<T> Executed(T? value) => new(value) { };
}
