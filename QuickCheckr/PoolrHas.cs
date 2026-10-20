namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Creates a conditional option that selects an element from a nonempty pool when executed.</summary>
    public static CheckrOf<CheckrOption> Has<T>(
        string key,
        Func<T, CheckrOf<Case>> checkr) =>
            When<T>(key, pool => pool.Count > 0, checkr);

    public static CheckrOf<CheckrOption> Has<T>(CheckrOf<Case> checkr) =>
        Has<T>().Then(checkr);

    /// <summary>Creates a deferred condition requiring at least one element in the pool.</summary>
    public static PoolCondition Has<T>() =>
        When<T>(pool => pool.Count > 0);

}
