namespace QuickCheckr.StringReduction.Bolts;

public abstract class BalancedSelector(char Open, char Close) : BaseSelector, ISelector
{
    public ISelection? Extract(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var left = input.IndexOf(Open);
        if (left >= 0)
        {
            var right = FindMatchingRight(input, left);
            if (right >= 0)
            {
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

public class InsideCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left + 1, right - left - 1);
}

public class EnclosedCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left, right - left + 1);
}

public class DelimiterCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new IndexSet([left, right]);
}