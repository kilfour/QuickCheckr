namespace QuickCheckr.StringReduction.Bolts;

public interface ISelection
{
    string Remove(string input);
}
public record Slice(int Start, int Length) : ISelection
{
    public string Remove(string input)
    {
        return input.Remove(Start, Length);
    }
}

public record IndexSet(int[] Indices) : ISelection
{
    public string Remove(string input)
    {
        return input.Remove(Indices[0], Indices[1] - Indices[0] + 1);
    }
}