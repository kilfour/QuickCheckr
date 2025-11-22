using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new ExecutionShrinkingStarted(State.ShortTermMemories.Count()));
        var shrinkCount = 0;
        for (var i = 0; i < State.ShortTermMemories.Numbers.Count && State.ShortTermMemories.Numbers.Count > 1; i++)
        {
            var id = State.ShortTermMemories.Numbers[i];
            State.Docket.Record(new ExecutionShrinkRunStarted(State.ShortTermMemories.Count()));
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                shrinkCount++;
                State.ShortTermMemories.Remove(id);
                i--;
                State.Docket.Record(new ExecutionRemoved(id));
            }
            State.Docket.Record(new ExecutionShrinkRunEnded(State.ShortTermMemories.Count()));
        }
        State.Docket.Record(new ExecutionShrinkingEnded(State.ShortTermMemories.Count()));
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        State.LongTermMemory.Reset();
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.ShortTermMemories.Numbers)
        {
            if (executionNumber != current)
                PerformExecution(executionNumber);
        }
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemories.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
