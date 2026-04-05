using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.StringReduction.Bolts;


public abstract record BalancedSelector(char Open, char Close) : ISelector
{
    public ISelection[] Extract(string input)
    {
        if (string.IsNullOrEmpty(input)) return [];
        List<ISelection>? selections = null;
        var left = input.IndexOf(Open);
        while (left >= 0)
        {
            var right = FindMatchingRight(input, left);
            if (right >= 0)
            {
                selections ??= [];
                selections.Add(GetSelection(left, right));
            }
            left = input.IndexOf(Open, left + 1);
        }
        return selections is null ? [] : [.. selections];
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

    public Func<Reducer<string>> Remove()
        => () => new MinimizeStringFactory(this).Reduce;

    private class MinimizeStringFactory(ISelector selector)
    {
        private int index = -1;
        private ISelection[] selections = [];
        public Reduction Reduce(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (index == -1)
                selections = selector.Extract(str);
            if (result.Passed)
                return new Reduction.Stop(str);
            index++;
            if (index >= selections.Length)
                return new Reduction.Unbounded(str);
            return new Reduction.Next(selections[index].Remove(str));
        }
    }
}

public record InsideCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left + 1, right - left - 1);
}

public record EnclosedCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new Slice(left, right - left + 1);
}

public record DelimiterCharSelector(char Open, char Close)
    : BalancedSelector(Open, Close), ISelector
{
    protected override ISelection GetSelection(int left, int right)
        => new IndexSet([left, right]);
}