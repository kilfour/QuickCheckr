using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<ITextSelection> Remove(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var result = selectr(state);
            if (!result.Succeeded)
                return SelectrResult.Default<ITextSelection>(state);

            var selection = result.Value;
            state.Remove(selection.AsSlice());
            return SelectrResult.Some(state, selection);
        };
}
