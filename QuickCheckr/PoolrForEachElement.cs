using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    private static CheckrOf<Case> ForEachElement<T>(Func<int, T, CheckrOf<Case>> checkr)
    {
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
                    await checkr(id, value)(state).ConfigureAwait(false);
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
