namespace QuickCheckr.StringReduction.Bolts;

public class OneOfSelector(ISelector left, char[] options) : BaseSelector, ISelector
{
    public ISelection? Extract(string text)
    {
        var index = 0;
        while (index < text.Length)
        {
            var selection = left.Extract(text[index..]);
            if (selection == null) break;
            if (selection is Slice slice)
            {
                var start = index + slice.Start;
                var matchIndex = start + slice.Length;
                if (matchIndex >= text.Length) break;
                if (options.Contains(text[matchIndex]))
                    return new Slice(start, slice.Length + 1);
                index = matchIndex;
                if (index == start)
                    index++;
                continue;
            }
            break;
        }
        return null;
    }
}
