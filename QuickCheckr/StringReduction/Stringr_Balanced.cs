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
                return SelectrResult.Default<ITextSelection>(state);

            var depth = 0;
            var maximumDepth = 0;
            var closingIndex = -1;
            for (var index = state.Index; index < state.Text.Length; index++)
            {
                if (state.Text[index] == left)
                {
                    depth++;
                    maximumDepth = Math.Max(maximumDepth, depth);
                }

                if (state.Text[index] == right)
                {
                    depth--;

                    if (depth == 0)
                    {
                        closingIndex = index;
                        break;
                    }
                }
            }

            if (closingIndex < 0)
                return SelectrResult.Default<ITextSelection>(state);

            var length = closingIndex - state.Index + 1;
            var result = SelectrResult.Some(state, StringSlice.From(state.Index, length, state.Text));
            if (maximumDepth == 1)
                state.Advance(length);
            else
                state.Advance();

            return result;
        };
}
