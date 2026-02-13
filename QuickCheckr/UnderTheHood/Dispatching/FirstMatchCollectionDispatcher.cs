namespace QuickCheckr.UnderTheHood.Dispatching;

public class FirstMatchCollectionDispatcher<TIn, TContext, TOut>(
    IReadOnlyList<FirstMatchHandler<TIn, TContext, TOut>> handlers,
    FirstMatchCollectionFallback<TContext, TOut> fallback)
{
    public TOut Dispatch(IEnumerable<TIn> inputs, TContext context)
    {
        foreach (var input in inputs)
        {
            foreach (var h in handlers)
            {
                var result = h.TryHandle(input, context);
                if (result.HasOutput)
                    return result.Output;
            }
        }
        return fallback.Handle(context);
    }
}
