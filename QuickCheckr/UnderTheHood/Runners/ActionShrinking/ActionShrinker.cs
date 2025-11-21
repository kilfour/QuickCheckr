using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

public class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new ActionShrinkingStarted(State.ShortTermMemory.Count()));
        var shrinkCount = 0;
        for (var i = 0; i < State.ShortTermMemory.Numbers.Count; i++)
        {
            var id = State.ShortTermMemory.Numbers[i];
            State.ShortTermMemory.GotoExecution(id);
            var oldKeys = State.ShortTermMemory.GetActions();
            if (oldKeys.Count < 1) continue;
            foreach (var key in oldKeys)
            {
                State.ShortTermMemory.RemoveAction(key);
                ExecuteRun();
                if (State.EvidenceRecorder.Failed())
                {
                    State.Docket.Record(new ActionRemoved(id, key));
                    shrinkCount++;
                }
                else
                {
                    State.ShortTermMemory.GotoExecution(id);
                    State.ShortTermMemory.StoreAction(key);
                }
            }
            State.Docket.Record(new ActionShrinkRunEnded(State.ShortTermMemory.Count()));
        }
        State.Docket.Record(new ActionShrinkingEnded(State.ShortTermMemory.Count()));
        return shrinkCount;
    }

    private void ExecuteRun()
    {
        State.Docket.Record(new ActionShrinkRunStarted(State.ShortTermMemory.Count()));
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.ShortTermMemory.Numbers)
        {
            PerformExecution(executionNumber);
        }
        State.Docket.Record(new ActionShrinkRunEnded(State.ShortTermMemory.Count()));
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemory.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
