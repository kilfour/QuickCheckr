using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Creates a conditional option that selects a pool element when executed.</summary>
    public static CheckrOf<CheckrOption> When<T>(
        string key,
        Func<IReadOnlyCollection<T>, bool> predicate,
        Func<T, CheckrOf<Case>> checkr) =>
            When(predicate).Then(
                from element in OneOfElement<T>(key)
                from result in checkr(element.Value)
                select result);

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
