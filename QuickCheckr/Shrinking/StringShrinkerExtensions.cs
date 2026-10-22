using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Shrinking;

public static class StringShrinkerExtensions
{
    /// <summary>Appends string selectors as fresh reducer factories.</summary>
    public static Shrinker<string> Reduce(
        this Shrinker<string> shrinker,
        params SelectrOf<Selection>[] selectors)
    {
        ArgumentNullException.ThrowIfNull(shrinker);
        ArgumentNullException.ThrowIfNull(selectors);
        return shrinker.Reduce(selectors.Select(selector => selector.Reduce()).ToArray());
    }
}
