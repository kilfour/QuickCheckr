using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Experimental;

public static class Taskr
{
    private static readonly CancellationTokenSource cancellationTokenSource = new();

    public static CheckrOf<Task> Start(string key, Func<CancellationToken, Task> action) =>
        state =>
        {
            state.Memory.StoreAction(key);
            var task = Task.Run(async () => await action(cancellationTokenSource.Token));
            var id = state.Memory.StoreVariable(key, () => Fuzzr.Int(0, int.MaxValue).Unique("Quick-Checkr-Pool-Id")(state.FuzzrState).Value);
            state.Memory.StoreInPool(id, task);
            return TheCatcher.InTheCheckr(state,
                () => CheckrResult.Some(state, task),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    public static CheckrOf<Task<T>> Start<T>(string key, Func<CancellationToken, Task<T>> action) =>
        state =>
        {
            state.Memory.StoreAction(key);
            var task = Task.Run(async () => await action(cancellationTokenSource.Token));
            var id = state.Memory.StoreVariable(key, () => Fuzzr.Int(0, int.MaxValue).Unique("Quick-Checkr-Pool-Id")(state.FuzzrState).Value);
            state.Memory.StoreInPool(id, task);
            return TheCatcher.InTheCheckr(state,
                () => CheckrResult.Some(state, task),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };
}

