using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class WordCount(CaseFile caseFile)
{
    private readonly List<CollapsableExecutionDeposition> depositions = caseFile.CollapseExecutions();
    public int Executions() => depositions.Count;
    public int Actions() => depositions.Sum(a => a.ExecutionDeposition.ActionDepositions.Count);
    public int Inputs() => depositions.Sum(a => a.ExecutionDeposition.InputDepositions.Count);
    public int PoolTraces() => depositions.Sum(a => a.ExecutionDeposition.PoolTraceDepositions.Count);
    public int Traces() => depositions.Sum(a => a.ExecutionDeposition.TraceDepositions.Count);
    public int Warnings() => depositions.Sum(a => a.ExecutionDeposition.WarningDepositions.Count);
    public int PassedExpectations() => caseFile.PassedExpectationDepositions.Count;
}