using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Creates a deferred condition over a pool's current contents.</summary>
    public static PoolCondition When<T>(Func<IReadOnlyCollection<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(state => predicate((IReadOnlyCollection<T>)state.Memory.GetPool<T>()));
    }

    public static CheckrOf<CheckrOption> When<T>(
        Func<IReadOnlyCollection<T>, bool> predicate,
        CheckrOf<Case> checkr) =>
            When(predicate).Then(checkr);

}
