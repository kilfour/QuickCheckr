namespace QuickCheckr.StringReduction.Nuts;



public record StringSlice(int Start, int Length, string Text) : ITextSelection, IEditable
{
    public static ITextSelection From(int start, int length, string input)
        => new StringSlice(start, length, input.Substring(start, length));

    public SelectrOf<IEditable> Replace(Func<string, string> replacementFactory)
    {
        return state =>
        {
            state.Changed = true;
            var replacement = replacementFactory(Text);
            state.LateReplace(this, replacement);
            return SelectrResult.Some(state, this as IEditable);
        };
    }
}
