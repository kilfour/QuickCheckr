using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check,
    Executions Executions)
{
    public int Run()
    {
        State.Docket.Record(new ExecutionShrinkingStarted(Executions.Count()));
        var shrinkCount = 0;
        for (var i = 0; i < Executions.Numbers.Count && Executions.Numbers.Count > 1; i++)
        {
            var id = Executions.Numbers[i];
            State.Docket.Record(new ExecutionShrinkRunStarted(Executions.Count()));
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                shrinkCount++;
                Executions.Remove(id);
                i--;
                State.Docket.Record(new ExecutionRemoved(id));
            }
            State.Docket.Record(new ExecutionShrinkRunEnded(Executions.Count()));
        }
        State.Docket.Record(new ExecutionShrinkingEnded(Executions.Count()));
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in Executions.Numbers)
        {
            if (executionNumber != current)
                PerformExecution(executionNumber);
        }
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemory.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
