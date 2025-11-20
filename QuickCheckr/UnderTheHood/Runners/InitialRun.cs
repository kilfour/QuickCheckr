using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners;

public class InitialRun(CheckrState State, CheckrOf<Case> Check)
{
    public Executions Executions { get; init; } = new();
    public Executions Run(int numberOfExecutions)
    {
        State.Docket.Record(new InitialRunStarted(numberOfExecutions));
        for (int j = 0; j < numberOfExecutions; j++)
        {
            PerformExecution();
            if (State.EvidenceRecorder.Failed())
            {
                State.Docket.Record(new InitialRunEnded(j + 1));
                return Executions;
            }
        }
        State.Docket.Record(new InitialRunEnded(numberOfExecutions));
        return Executions;
    }

    private void PerformExecution()
    {
        var executionNumber = Executions.Next();
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemory.StartNewExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}