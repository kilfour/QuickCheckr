using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    public static SelectrOf<ITextSelection> Balanced(char left, char right) =>
        state =>
        {
            if (state.OutOfBounds())
                return SelectrResult.Default<ITextSelection>(state);
            if (state.Text[state.Index] != left)
            {
                state.RunStopped = true;
                return SelectrResult.Default<ITextSelection>(state);
            }
            var length = 1;
            int depth = 1;
            int maxDepth = 1;
            while (state.InBounds(length + 1) && depth != 0)
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
                return SelectrResult.Default<ITextSelection>(state);
            }
            var result = SelectrResult.Some(state, StringSlice.From(state.Index, length + 1, state.Text));
            if (maxDepth == 1)
                state.Index += length + 1;
            else
                state.Index++;
            return result;
        };
}
