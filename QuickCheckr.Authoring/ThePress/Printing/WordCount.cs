using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class WordCount(IRecord record)
{
    private readonly List<CollapsableExecutionDeposition> depositions =
        record is CaseFile caseFile ? caseFile.CollapseExecutions() : [];

    public int Executions() => depositions.Count;
    public int Actions() => depositions.Sum(a => a.ExecutionDeposition.ActionDepositions.Count);
    public int Inputs() => depositions.Sum(a => a.ExecutionDeposition.InputDepositions.Count);
    public int PoolTraces() => depositions.Sum(a => a.ExecutionDeposition.PoolTraceDepositions.Count);
    public int Traces() => depositions.Sum(a => a.ExecutionDeposition.TraceDepositions.Count);
    public int FinalTraces() => depositions.Sum(a => a.ExecutionDeposition.FinalTraceDepositions.Count);
    public int Warnings() => depositions.Sum(a => a.ExecutionDeposition.WarningDepositions.Count);
    public int PassedExpectations() =>
        record is CaseFile caseFile ? caseFile.PassedExpectations.Count : ((Findings)record).PassedExpectations.Count;
}