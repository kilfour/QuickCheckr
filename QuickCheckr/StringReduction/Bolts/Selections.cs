namespace QuickCheckr.StringReduction.Bolts;

public interface ISelection
{
    string GetText(string input);
    string Remove(string input);
    string Replace(string input, string replacement);
}

public record Slice(int Start, int Length) : ISelection
{
    public string GetText(string input)
        => input.Substring(Start, Length);

    public string Remove(string input)
        => input.Remove(Start, Length);

    public string Replace(string input, string replacement)
        => input[..Start] + replacement + input[(Start + Length)..];
}

public record IndexSet(int[] Indices) : ISelection
{
    public string GetText(string input)
        => string.Empty;

    public string Remove(string input)
    {
        var chars = input.ToList();
        foreach (var i in Indices.OrderByDescending(x => x))
        {
            if (i >= 0 && i < chars.Count)
                chars.RemoveAt(i);
        }
        return new string([.. chars]);
    }

    public string Replace(string input, string replacement)
        => throw new InvalidOperationException();
}