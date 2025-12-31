
namespace QuickCheckr.UnderTheHood.Proceedings;

public record ExecutionDeposition
{
    public int ExecutionId { get; }

    public int Times { get; private set; } = 1;

    public List<ActionDeposition> ActionDepositions { get; } = [];

    public List<InputDeposition> InputDepositions { get; } = [];

    public List<TraceDeposition> TraceDepositions { get; } = [];

    public List<PoolTraceDeposition> PoolTraceDepositions { get; } = [];

    public List<WarningDeposition> WarningDepositions { get; } = [];

    public ExecutionDeposition(int executionId)
    {
        ExecutionId = executionId;
    }

    public bool Collapsed(ExecutionDeposition other)
    {
        if (!ActionDepositions.SequenceEqual(other.ActionDepositions))
            return false;
        if (!InputDepositions.SequenceEqual(other.InputDepositions))
            return false;
        if (!PoolTraceDepositions.SequenceEqual(other.PoolTraceDepositions))
            return false;
        if (!TraceDepositions.SequenceEqual(other.TraceDepositions))
            return false;
        if (!WarningDepositions.SequenceEqual(other.WarningDepositions))
            return false;
        Times++;
        return true;
    }

    public bool HasContent()
    {
        if (ActionDepositions.Count > 0) return true;
        if (InputDepositions.Count > 0) return true;
        if (PoolTraceDepositions.Count > 0) return true;
        if (TraceDepositions.Count > 0) return true;
        if (WarningDepositions.Count > 0) return true;
        return false;
    }

    public ExecutionDeposition AddActionDeposition(ActionDeposition actionDeposition)
    {
        ActionDepositions.Add(actionDeposition);
        return this;
    }

    public ExecutionDeposition AddInputDeposition(InputDeposition inputDeposition)
    {
        InputDepositions.Add(inputDeposition);
        return this;
    }

    public ExecutionDeposition AddTraceDeposition(TraceDeposition traceDeposition)
    {
        TraceDepositions.Add(traceDeposition);
        return this;
    }

    public ExecutionDeposition AddPoolTraceDeposition(PoolTraceDeposition poolTraceDeposition)
    {
        PoolTraceDepositions.Add(poolTraceDeposition);
        return this;
    }

    public ExecutionDeposition AddWarningDeposition(WarningDeposition warningDeposition)
    {
        WarningDepositions.Add(warningDeposition);
        return this;
    }

    public int MaxLabelLength() =>
        new int[]
            { PoolTraceDepositions.Count == 0 ? 0 : PoolTraceDepositions.Max(a => a.Label.Length)
            , InputDepositions.Count == 0 ? 0 : InputDepositions.Max(a => a.Label.Length)
            , TraceDepositions.Count == 0 ? 0 : TraceDepositions.Max(a => a.Label.Length)
            }.Max();
};