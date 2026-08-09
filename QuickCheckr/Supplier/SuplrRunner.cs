namespace QuickCheckr.Supplier;

public static class SuplrRunner
{
    public static ISupplrDefinition<T> Resolve<T>(this SupplrOf<T> supplr) =>
        supplr(new SupplrState()).Definition;
}
