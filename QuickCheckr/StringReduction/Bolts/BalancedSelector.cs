namespace QuickCheckr.StringReduction.Bolts;

public abstract class BalancedSelector(char Open, char Close, ISelector? leftSelector) : BaseSelector, ISelector
{
    public ISelection? Extract(string text)
    {
        var keep = false;
        var search = 0;
        var start = 0;
        if (leftSelector != null)
        {
            var selection = leftSelector.Extract(text);
            if (selection is Slice slice)
            {
                search = slice.Start + slice.Length;
                start = slice.Start;
                keep = true;
            }
            else return null;
        }
        if (string.IsNullOrEmpty(text)) return null;
        var left = text.IndexOf(Open, start);
        if (left >= 0)
        {
            var right = FindMatchingRight(text, left);
            if (right >= 0)
            {
                if (keep)
                {
                    if (left != search)
                        return null;
                    else
                        return GetSelection(start, right);
                }
                return GetSelection(left, right);
            }
        }
        return null;
    }

    private int FindMatchingRight(string text, int leftIndex)
    {
        int depth = 0;
        for (int i = leftIndex; i < text.Length; i++)
        {
            var current = text[i];
            if (current == Open)
                depth++;
            if (current == Close)
                depth--;
            if (depth == 0)
                return i;
        }
        return -1;
    }

    protected abstract ISelection GetSelection(int left, int right);
}

public class InsideCharSelector(char Open, char Close, ISelector? left)
    : BalancedSelector(Open, Close, left), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left + 1, right - left - 1);
}

public class EnclosedCharSelector(char Open, char Close, ISelector? left)
    : BalancedSelector(Open, Close, left), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left, right - left + 1);
}

public class DelimiterCharSelector(char Open, char Close, ISelector? left)
    : BalancedSelector(Open, Close, left), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new IndexSet([left, right]);
}