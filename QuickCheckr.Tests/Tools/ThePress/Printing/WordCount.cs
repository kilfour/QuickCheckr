using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class WordCount(CaseFile caseFile)
{
    private readonly List<ExecutionDeposition> depositions = caseFile.Verdict!.ExecutionDepositions;
    public int Executions() => depositions.Count;
    public int Actions() => depositions.Sum(a => a.ActionDepositions.Count);
    public int Inputs() => depositions.Sum(a => a.InputDepositions.Count);
    public int Traces() => depositions.Sum(a => a.TraceDepositions.Count);
    public int Warnings() => depositions.Sum(a => a.WarningDepositions.Count);
    public int PassedSpecs() => caseFile.PassedExpectationDepositions.Count;
}