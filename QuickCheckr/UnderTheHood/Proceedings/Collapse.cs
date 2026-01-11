using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.UnderTheHood.Proceedings;

public static class Collapse
{
    public static List<CollapsableExecutionDeposition> TheExecutionDepositions(List<ExecutionDeposition> executionDepositions)
    {
        CollapsableExecutionDeposition? collapsableExecutionDeposition = null;
        var result = new List<CollapsableExecutionDeposition>();
        foreach (var executionDeposition in executionDepositions)
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
}