namespace QuickCheckr.UnderTheHood.Dispatching;

internal abstract class FirstMatchFallback<TIn, TContext, TOut>
{
    public abstract TOut Handle(TIn input, TContext context);
}
