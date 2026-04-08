using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    public static SelectrOf<ITextSelection> Once(params char[] options) =>
        Once(a => options.Contains(a));


    public static SelectrOf<ITextSelection> Once(Func<char, bool> predicate) =>
        state =>
        {
            if (state.OutOfBounds()) return SelectrResult.Default<ITextSelection>(state);
            if (predicate(state.Text[state.Index]))
            {
                var result = StringSlice.From(state.Index, 1, state.Text);
                state.Advance();
                return SelectrResult.Some(state, result);
            }
            state.RunStopped = true;
            return SelectrResult.Default<ITextSelection>(state);
        };
}
