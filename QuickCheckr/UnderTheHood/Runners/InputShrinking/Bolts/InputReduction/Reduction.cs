namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public abstract record Reduction
{
    public record Next(object Candidate) : Reduction;
    public record Stop(object Result) : Reduction;
    public sealed record Unbounded(object Result) : Reduction;
}