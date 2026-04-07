namespace QuickCheckr.StringReduction.Nuts;

public class SelectrRun(SelectrOf<Selection> selectr, string text)
{
    private readonly SelectrState state = new(text);
    private int outerIndex = 0;

    public string Next()
    {
        while (!EndOfText)
        {
            while (!EndOfText)
            {
                var prevText = state.Text;
                state.NewRun = true;
                state.RunStopped = false;
                selectr(state);
                if (state.RunStopped)
                    state.Text = prevText;
                else
                    return state.Text;
                state.Index++;
            }
            if (EndOfText)
                state.Index = outerIndex++;
        }
        return state.Text;
    }

    public bool EndOfText =>
        state.Index >= state.Text.Length;
}
