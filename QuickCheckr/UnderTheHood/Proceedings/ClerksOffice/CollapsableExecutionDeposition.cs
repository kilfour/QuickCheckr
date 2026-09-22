using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

internal sealed record CollapsableExecutionDeposition(ExecutionDeposition ExecutionDeposition)
{
    public int Times { get; private set; } = 1;
    public bool Collapsed(ExecutionDeposition other)
    {
        if (!ExecutionDeposition.ActionDepositions.SequenceEqual(other.ActionDepositions))
            return false;
        if (!ExecutionDeposition.InputDepositions.SequenceEqual(other.InputDepositions))
            return false;
        if (!ExecutionDeposition.PoolTraceDepositions.SequenceEqual(other.PoolTraceDepositions))
            return false;
        if (!ExecutionDeposition.TraceDepositions.SequenceEqual(other.TraceDepositions))
            return false;
        if (!ExecutionDeposition.WarningDepositions.SequenceEqual(other.WarningDepositions))
            return false;
        Times++;
        return true;
    }

}
