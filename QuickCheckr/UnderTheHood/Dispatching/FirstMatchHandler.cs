namespace QuickCheckr.UnderTheHood.Dispatching;

internal abstract class FirstMatchHandler<TIn, TContext, TOut>
{
    protected HandlerResult<TOut> Unhandled => HandlerResult.Unhandled<TOut>();
    public abstract ValueTask<HandlerResult<TOut>> TryHandleAsync(TIn input, TContext context);
}
