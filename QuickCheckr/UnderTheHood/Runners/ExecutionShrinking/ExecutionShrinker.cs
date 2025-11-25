using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new ExecutionShrinkingStarted(State.Memory.ShortTermMemory.Count()));
        var shrinkCount = 0;
        for (var i = 0; i < State.Memory.ShortTermMemory.Numbers.Count && State.Memory.ShortTermMemory.Numbers.Count > 1; i++)
        {
            var id = State.Memory.ShortTermMemory.Numbers[i];
            State.Docket.Record(new ExecutionShrinkRunStarted(State.Memory.ShortTermMemory.Count()));
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                shrinkCount++;
                State.Memory.ShortTermMemory.Remove(id);
                i--;
                State.Docket.Record(new ExecutionRemoved(id));
            }
            State.Docket.Record(new ExecutionShrinkRunEnded(State.Memory.ShortTermMemory.Count()));
        }
        State.Docket.Record(new ExecutionShrinkingEnded(State.Memory.ShortTermMemory.Count()));
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        State.Memory.LongTermMemory.Reset();
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.Memory.ShortTermMemory.Numbers)
        {
            if (executionNumber != current)
                PerformExecution(executionNumber);
        }
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.Memory.ShortTermMemory.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
