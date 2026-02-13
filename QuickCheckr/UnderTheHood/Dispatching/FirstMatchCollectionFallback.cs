namespace QuickCheckr.UnderTheHood.Dispatching;

public abstract class FirstMatchCollectionFallback<TContext, TOut>
{
    public abstract TOut Handle(TContext context);
}