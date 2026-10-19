namespace QuickCheckr;

public static partial class Poolr
{
    public static CheckrOf<CheckrOption> Has<T>(CheckrOf<Case> checkr) =>
        Has<T>().Then(checkr);

    /// <summary>Creates a deferred condition requiring at least one element in the pool.</summary>
    public static PoolCondition Has<T>() =>
        When<T>(pool => pool.Count > 0);

}
