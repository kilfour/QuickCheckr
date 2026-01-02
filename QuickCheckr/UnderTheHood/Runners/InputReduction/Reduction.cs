namespace QuickCheckr.UnderTheHood.Runners.InputReduction;

public record Reduction
{
    public bool Continue { get; init; }
    public object? Candidate { get; init; }

    public static Reduction Next(object candidate)
        => new() { Continue = true, Candidate = candidate };
    public static Reduction Stop()
        => new() { Continue = false };
};
