namespace QuickCheckr.UnderTheHood.Dispatching;

public abstract class FirstMatchFallback<TIn, TContext, TOut>
{
    public abstract TOut Handle(TIn input, TContext context);
}