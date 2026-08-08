using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

namespace QuickCheckr;

public static partial class Shrink
{
    public static Shrinker Collapse<T>(params Func<T, T?>[] selectors)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(selectors);

        if (selectors.Length == 0)
            throw new ArgumentException("At least one collapse selector is required.", nameof(selectors));
        if (selectors.Any(selector => selector is null))
            throw new ArgumentException("Collapse selectors cannot contain null.", nameof(selectors));

        return new Shrinker(
            info => typeof(T).IsAssignableFrom(info.ActualType),
            [],
            [],
            [])
        {
            Collapsers = [.. selectors.Select(selector => new CollapseOption<T>(selector))]
        };
    }
}
