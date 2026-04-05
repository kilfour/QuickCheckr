namespace QuickCheckr.StringReduction.Bolts;

public class SkipWhile(Func<char, bool> predicate) : BaseSelector, ISelector
{
    public ISelection? Extract(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (!predicate(text[i]))
                return new Slice(i, 0);
        }
        return null;
    }
}