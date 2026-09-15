namespace QuickCheckr.UnderTheHood.Dispatching;

internal class FirstMatchDispatcher<TIn, TContext, TOut>(
    IEnumerable<FirstMatchHandler<TIn, TContext, TOut>> handlers,
    FirstMatchFallback<TIn, TContext, TOut> fallback)
{
    private readonly IEnumerable<FirstMatchHandler<TIn, TContext, TOut>> chain = [.. handlers];
    public async ValueTask<TOut> DispatchAsync(TIn input, TContext context)
    {
        foreach (var handler in chain)
        {
            var result = await handler.TryHandleAsync(input, context).ConfigureAwait(false);
            if (result.HasOutput)
                return result.Output;
        }
        return fallback.Handle(input, context);
    }
}
