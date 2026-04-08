using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<ITextSelection> Remove(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var selection = selectr(state).Value;
            if (selection == null) return SelectrResult.Default<ITextSelection>(state);
            state.Remove(selection.AsSlice());
            return SelectrResult.Some(state, selection);
        };
}
