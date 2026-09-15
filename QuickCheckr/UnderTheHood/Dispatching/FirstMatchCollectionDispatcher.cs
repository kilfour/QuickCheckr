namespace QuickCheckr.UnderTheHood.Dispatching;

internal class FirstMatchCollectionDispatcher<TIn, TContext, TOut>(
    IReadOnlyList<FirstMatchHandler<TIn, TContext, TOut>> handlers,
    FirstMatchCollectionFallback<TContext, TOut> fallback)
{
    public async ValueTask<TOut> DispatchAsync(IEnumerable<TIn> inputs, TContext context)
    {
        foreach (var input in inputs)
        {
            foreach (var h in handlers)
            {
                var result = await h.TryHandleAsync(input, context).ConfigureAwait(false);
                if (result.HasOutput)
                    return result.Output;
            }
        }
        return fallback.Handle(context);
    }
}
