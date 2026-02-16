namespace QuickCheckr.UnderTheHood.Dispatching;

public class FirstMatchDispatcher<TIn, TContext, TOut>(
    IEnumerable<FirstMatchHandler<TIn, TContext, TOut>> handlers,
    FirstMatchFallback<TIn, TContext, TOut> fallback)
{
    private readonly IEnumerable<FirstMatchHandler<TIn, TContext, TOut>> chain = [.. handlers];
    public TOut Dispatch(TIn input, TContext context)
    {
        foreach (var handler in chain)
        {
            var result = handler.TryHandle(input, context);
            if (result.HasOutput)
                return result.Output;
        }
        return fallback.Handle(input, context);
    }
}
