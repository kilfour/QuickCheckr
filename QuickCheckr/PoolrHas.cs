namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Creates a conditional option that selects an element from a nonempty pool when executed.</summary>
    public static CheckrOf<CheckrOption> Has<T>(
        string key,
        Func<T, PoolEntryOperations<T>, CheckrOf<Case>> checkr) =>
            When(key, a => true, checkr);

    public static CheckrOf<CheckrOption> Has<T>(
        string key,
        Func<T, CheckrOf<Case>> checkr) =>
            When(key, pool => pool.Count > 0, checkr);

    public static PoolCondition Has<T>() =>
        When<T>(pool => pool.Count > 0);
    
    public static CheckrOf<CheckrOption> Has<T>(CheckrOf<Case> checkr) =>
        Has<T>().Then(checkr);
}
