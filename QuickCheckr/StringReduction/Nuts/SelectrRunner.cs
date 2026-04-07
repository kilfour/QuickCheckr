namespace QuickCheckr.StringReduction.Nuts;

public static class SelectrRunner
{

    public class SelectrRun(SelectrOf<Selection> selectr, string text)
    {
        private readonly SelectrState state = new(text);
        public string Next()
        {
            state.PrevText = state.Text;
            state.NewRun = true;
            state.RunStopped = false;
            selectr(state);
            if (state.RunStopped)
            {
                state.Text = state.PrevText;
                return state.Text;
            }
            return state.Text;
        }
    }
    public static SelectrRun For(SelectrOf<Selection> selectr, string text)
        => new(selectr, text);

    public static SelectrRun Run(this SelectrOf<Selection> selectr, string text)
        => new(selectr, text);
}
