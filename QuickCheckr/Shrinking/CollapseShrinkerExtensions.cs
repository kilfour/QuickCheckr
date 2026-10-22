using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

namespace QuickCheckr.Shrinking;

public static class CollapseShrinkerExtensions
{
    /// <summary>Appends descendant selectors to try before classification.</summary>
    public static Shrinker<T> Collapse<T>(this Shrinker<T> shrinker, params Func<T, T?>[] selectors)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(shrinker);
        ArgumentNullException.ThrowIfNull(selectors);

        if (selectors.Length == 0)
            throw new ArgumentException("At least one collapse selector is required.", nameof(selectors));
        if (selectors.Any(selector => selector is null))
            throw new ArgumentException("Collapse selectors cannot contain null.", nameof(selectors));

        return shrinker.AppendCollapsers([.. selectors.Select(selector => new CollapseOption<T>(selector))]);
    }
}
