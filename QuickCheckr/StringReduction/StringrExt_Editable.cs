using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<IEditable> Editable(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var selection = selectr(state).Value;
            if (selection == null) return SelectrResult.Default<IEditable>(state);
            return SelectrResult.Some(state, selection.AsEditable());
        };
}
