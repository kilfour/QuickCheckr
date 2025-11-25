using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners;

public class InitialRun(CheckrState State, CheckrOf<Case> Check)
{
    public void Run(int numberOfExecutions)
    {
        State.Docket.Record(new InitialRunStarted(numberOfExecutions));
        for (int j = 0; j < numberOfExecutions; j++)
        {
            PerformExecution();
            if (State.EvidenceRecorder.Failed())
            {
                State.Docket.Record(new InitialRunEnded(j + 1));
                return;
            }
        }
        State.Docket.Record(new InitialRunEnded(numberOfExecutions));
    }

    private void PerformExecution()
    {
        var executionNumber = State.Memory.StartNewExecution();
        State.Docket.Record(new ExecutionStarted(executionNumber));
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}