namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

/// <summary>
/// Describes how a reducer should continue after testing a candidate.
/// </summary>
public abstract record Reduction
{
    public sealed record Next(object Candidate) : Reduction;
    public sealed record Stop(object Result) : Reduction;
    public sealed record Unbounded(object Result) : Reduction;
}
