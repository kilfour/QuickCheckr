using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class CaseFileReportingExtensions
{
    public static List<CollapsableExecutionDeposition> CollapseExecutions(this CaseFile caseFile)
    {
        CollapsableExecutionDeposition? collapsableExecutionDeposition = null;
        var result = new List<CollapsableExecutionDeposition>();
        foreach (var executionDeposition in caseFile.ExecutionDepositions)
        {
            if (collapsableExecutionDeposition == null)
            {
                collapsableExecutionDeposition = new CollapsableExecutionDeposition(executionDeposition);
                result.Add(collapsableExecutionDeposition);
            }
            else
            {
                if (collapsableExecutionDeposition.Collapsed(executionDeposition))
                    continue;
                collapsableExecutionDeposition = new CollapsableExecutionDeposition(executionDeposition);
                result.Add(collapsableExecutionDeposition);
            }
        }
        return result;
    }

    public static int MaxLabelLength(this ExecutionDeposition executionDeposition) =>
        new int[]
            { executionDeposition.PoolTraceDepositions.Count == 0 ? 0 : executionDeposition.PoolTraceDepositions.Max(a => a.Label.Length)
            , executionDeposition.InputDepositions.Count == 0 ? 0 : executionDeposition.InputDepositions.Max(a => a.MaxLabelLength())
            , executionDeposition.TraceDepositions.Count == 0 ? 0 : executionDeposition.TraceDepositions.Max(a => a.Label.Length)
            }.Max();
}