namespace QuickCheckr.StringReduction.Nuts;

public record SelectrSlice(int Start, int Length, string Text)
{
    public static SelectrSlice From(int start, int length, string input)
        => new(start, length, input.Substring(start, length));

    public string GetText(string input) => Text;

    public string Remove(string input)
        => input.Remove(Start, Length);

    public string Replace(string input, string replacement, int offset = 0)
        => input[..(Start + offset)] + replacement + input[(Start + Length + offset)..];

    public SelectrOf<string> Replace(Func<string, string> replacementFactory)
    {
        return state =>
        {
            var replacement = replacementFactory(Text);
            state.Text = Replace(state.Text, replacement, state.Offset);
            state.Offset += replacement.Length - Length;
            return SelectrResult.Some(state, replacement);
        };
    }
}
