using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Runs a Checkr for each pool value. Expectations with the same label are reported once,
    /// with the pool identifiers of failing elements included in their messages.
    /// </summary>
    public static CheckrOf<Case> ForEach<T>(Func<T, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return async state =>
        {
            var elements = state.Memory.GetPoolDictionary<T>().ToArray();
            var parent = state.PoolIteration;
            var scope = new PoolIterationScope(state, parent);
            state.PoolIteration = scope;
            try
            {
                foreach (var (id, value) in elements)
                {
                    state.CancellationToken.ThrowIfCancellationRequested();
                    if (state.RunStopped) break;
                    scope.StartElement($"{typeof(T).Name}-{id}");
                    await checkr(value)(state).ConfigureAwait(false);
                }
                state.CancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                state.PoolIteration = parent;
            }
            if (!state.RunStopped)
                scope.Complete();
            return CheckrResult.CaseOnly(state);
        };
    }
}
