using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>A deferred pool condition that can be combined before attaching a Checkr.</summary>
public sealed class PoolCondition
{
    private readonly Func<CheckrState, bool> predicate;

    internal PoolCondition(Func<CheckrState, bool> predicate)
        => this.predicate = predicate;

    /// <summary>Requires both conditions, evaluating the second only when the first passes.</summary>
    public PoolCondition AndWhen<T>(Func<IReadOnlyCollection<T>, bool> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new(state => predicate(state) && other((IReadOnlyCollection<T>)state.Memory.GetPool<T>()));
    }

    public PoolCondition AndHas<T>() =>
        AndWhen<T>(pool => pool.Count > 0);

    /// <summary>Creates an option whose condition is evaluated against the current execution's pools.</summary>
    public CheckrOf<CheckrOption> Then(CheckrOf<Case> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return state => CheckrResult.Some(state,
            new CheckrOption(() => predicate(state), checkr));
    }
}
