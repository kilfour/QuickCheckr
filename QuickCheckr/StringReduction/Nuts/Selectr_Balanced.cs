using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;


public static partial class Selectr
{
    public static SelectrOf<SelectrSlice> Balanced(char left, char right) =>
        state =>
        {
            if (state.OutOfBounds())
                return SelectrResult.Default<SelectrSlice>(state);
            if (state.Text[state.Index] != left)
            {
                state.RunStopped = true;
                return SelectrResult.Default<SelectrSlice>(state);
            }
            var length = 1;
            int depth = 1;
            int maxDepth = 1;
            while (state.InBounds(length) && depth != 0)
            {
                length++;
                if (state.Text[state.Index + length] == left)
                {
                    depth++;
                    maxDepth++;
                }
                if (state.Text[state.Index + length] == right)
                    depth--;
            }
            if (state.OutOfBounds(length) || state.Text[state.Index + length] != right)
            {
                state.RunStopped = true;
                return SelectrResult.Default<SelectrSlice>(state);
            }
            var result = SelectrResult.Some(state, SelectrSlice.From(state.Index, length + 1, state.Text));
            if (maxDepth == 1)
                state.Index += length + 1;
            else
                state.Index++;
            return result;
        };
}
