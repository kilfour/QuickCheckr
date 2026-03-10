namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;

public record PoolCondition<TPoolElement>(
    Func<IReadOnlyCollection<TPoolElement>, bool>? OnCollection,
    Func<TPoolElement, bool>? OnElement
)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> GetCheckr(
        string label,
        Func<PoolElement<TPoolElement>, CheckrOf<Case>> checkr)
    {
        if (OnCollection != null)
            return Trackr.PoolWhen(label, OnCollection, checkr);
        if (OnElement != null)
            return Trackr.PoolWhen(label, OnElement, checkr);
        throw new InvalidOperationException();
    }
}
