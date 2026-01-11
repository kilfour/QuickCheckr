using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class ExecutionArticle(CollapsableExecutionDeposition collapsableExecutionDeposition)
    : AbstractArticle<ExecutionDeposition>(collapsableExecutionDeposition.ExecutionDeposition)
{
    [StackTraceHidden]
    public ActionArticle Action(int number)
        => new(deposition.ActionDepositions[number - 1]);

    [StackTraceHidden]
    public InputArticle Input(int number)
        => new(deposition.InputDepositions[number - 1]);

    [StackTraceHidden]
    public PoolTraceArticle PoolTrace(int number)
        => new(deposition.PoolTraceDepositions[number - 1]);

    [StackTraceHidden]
    public TraceArticle Trace(int number)
        => new(deposition.TraceDepositions[number - 1]);

    [StackTraceHidden]
    public WarningArticle Warning(int number)
        => new(deposition.WarningDepositions[number - 1]);

    public int Times => collapsableExecutionDeposition.Times;
}
