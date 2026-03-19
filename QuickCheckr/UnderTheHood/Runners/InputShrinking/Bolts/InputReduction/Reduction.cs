namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

// public record Reduction
// {
//     public bool Continue { get; init; }
//     public object? Candidate { get; init; }
//     public object? Result { get; init; }

//     public static Reduction Next(object candidate)
//         => new() { Continue = true, Candidate = candidate };
//     public static Reduction Stop(object result)
//         => new() { Continue = false, Result = result };
// }
public abstract record Reduction
{
    public record Next(object Candidate) : Reduction;
    public record Stop(object Result, object? Witness = null) : Reduction;
}