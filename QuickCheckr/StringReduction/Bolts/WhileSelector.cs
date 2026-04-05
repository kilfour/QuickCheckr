namespace QuickCheckr.StringReduction.Bolts;

public class WhileSelector(Func<char, bool> predicate, ISelector? left = null) : BaseSelector, ISelector
{
    public ISelection? Extract(string text)
    {
        var keep = false;
        var search = 0;
        var start = 0;
        var length = 1;
        ISelection? selection = null;
        if (left != null)
        {
            selection = left.Extract(text);
            if (selection is Slice slice)
            {
                search = slice.Start + slice.Length;
                start = slice.Start;
                length = slice.Length;
                keep = true;
            }
            else return null;
        }
        for (int i = search; i < text.Length; i++)
        {
            if (predicate(text[i]))
            {
                if (keep)
                    length++;
                else
                {
                    start = i;
                    keep = true;
                }
            }
            else if (keep) return new Slice(start, length);
        }
        if (keep) return new Slice(start, length);
        if (selection is Slice { Length: 0 })
            return null;
        return selection;
    }
}
