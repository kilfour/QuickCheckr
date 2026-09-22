namespace QuickCheckr.FilingCabinet.Depositions;

public sealed record TraceDeposition(string Label, string[] Value)
{
    public bool Equals(TraceDeposition? other) =>
        other is not null &&
        Label == other.Label &&
        Value.SequenceEqual(other.Value);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Label);
        foreach (var line in Value)
            hash.Add(line);
        return hash.ToHashCode();
    }
}
