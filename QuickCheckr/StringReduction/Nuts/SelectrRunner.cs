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
                state.StoreText();
                state.RunStopped = false;
                selectr(state);
                if (state.RunStopped)
                    state.RestoreText();
                else
                {
                    if (!EndOfText && state.Changed)
                        state.Restart(outerIndex);
                    return state.Text;
                }
                state.Advance();
            }
            if (EndOfText)
            {
                outerIndex++;
                state.Restart(outerIndex);
            }
        }
        return state.Text;
    }

    public bool EndOfText =>
        state.Index >= state.Text.Length;

    public bool Changed => state.Changed;
}
