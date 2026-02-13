namespace QuickCheckr.UnderTheHood.Dispatching;

public abstract class FirstMatchHandler<TIn, TContext, TOut>
{
    protected HandlerResult<TOut> Unhandled => HandlerResult.Unhandled<TOut>();
    public abstract HandlerResult<TOut> TryHandle(TIn input, TContext context);
}
