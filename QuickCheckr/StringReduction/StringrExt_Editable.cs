using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    /// <summary>
    /// Retains the selected range so it can be replaced by a later query step.
    /// Use when a replacement depends on text captured elsewhere in the same LINQ query.
    /// </summary>
    public static SelectrOf<IEditable> Editable(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var result = selectr(state);
            if (!result.Succeeded)
                return SelectrResult.Default<IEditable>(state);

            return SelectrResult.Some(state, result.Value.AsEditable());
        };
}
