using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<IEditable> Editable(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var result = selectr(state);
            if (!result.Succeeded)
                return SelectrResult.Default<IEditable>(state);

            return SelectrResult.Some(state, result.Value.AsEditable());
        };
}
