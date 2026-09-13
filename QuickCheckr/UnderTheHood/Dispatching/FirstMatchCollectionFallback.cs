namespace QuickCheckr.UnderTheHood.Dispatching;

internal abstract class FirstMatchCollectionFallback<TContext, TOut>
{
    public abstract TOut Handle(TContext context);
}
