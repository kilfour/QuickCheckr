using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class ExecutionArticle(ExecutionDeposition deposition)
    : AbstractArticle<ExecutionDeposition>(deposition)
{
    [StackTraceHidden]
    public ActionArticle Action(int number)
        => new(deposition.ActionDepositions[number - 1]);

    [StackTraceHidden]
    public InputArticle Input(int number)
        => new(deposition.InputDepositions[number - 1]);

    [StackTraceHidden]
    public TraceArticle Trace(int number)
        => new(deposition.TraceDepositions[number - 1]);
}
