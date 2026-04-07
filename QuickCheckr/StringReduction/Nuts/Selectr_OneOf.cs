using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;


public static partial class Selectr
{
    public static SelectrOf<ISelection> OneOf(params char[] options) =>
        state =>
        {
            if (state.NewRun)
            {
                state.NewRun = false;
                for (int i = state.Index; i < state.Text.Length; i++)
                {
                    if (options.Contains(state.Text[i]))
                    {
                        state.Index = i;
                        break;
                    }
                }
            }
            if (options.Contains(state.Text[state.Index]))
            {
                var result = new Slice(state.Index, 1);
                state.Index++;
                return SelectrResult.Some<ISelection>(state, result);
            }
            state.RunStopped = true;
            return SelectrResult.Default<ISelection>(state);
        };
}
