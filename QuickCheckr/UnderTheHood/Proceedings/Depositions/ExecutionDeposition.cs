namespace QuickCheckr.UnderTheHood.Proceedings.Depositions;

public record ExecutionDeposition
{
    public int ExecutionId { get; }

    public List<ActionDeposition> ActionDepositions { get; } = [];

    public List<InputDeposition> InputDepositions { get; } = [];

    public List<TraceDeposition> TraceDepositions { get; } = [];

    public List<TraceDeposition> FinalTraceDepositions { get; } = [];

    public List<PoolTraceDeposition> PoolTraceDepositions { get; } = [];

    public List<WarningDeposition> WarningDepositions { get; } = [];

    public ExecutionDeposition(int executionId)
    {
        ExecutionId = executionId;
    }

    public bool HasContent()
    {
        if (ActionDepositions.Count > 0) return true;
        if (InputDepositions.Count > 0) return true;
        if (PoolTraceDepositions.Count > 0) return true;
        if (TraceDepositions.Count > 0) return true;
        if (FinalTraceDepositions.Count > 0) return true;
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

    public ExecutionDeposition AddFinalTraceDeposition(TraceDeposition traceDeposition)
    {
        FinalTraceDepositions.Add(traceDeposition);
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
};