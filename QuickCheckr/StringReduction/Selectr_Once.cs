using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Selectr
{
    public static SelectrOf<SelectrSlice> Once(params char[] options) =>
        Once(a => options.Contains(a));

    public static SelectrOf<SelectrSlice> Once(Func<char, bool> predicate) =>
        state =>
        {
            if (state.OutOfBounds()) return SelectrResult.Default<SelectrSlice>(state);
            if (predicate(state.Text[state.Index]))
            {
                var result = SelectrSlice.From(state.Index, 1, state.Text);
                state.Index++;
                return SelectrResult.Some<SelectrSlice>(state, result);
            }
            state.RunStopped = true;
            return SelectrResult.Default<SelectrSlice>(state);
        };
}
