using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

public class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new ActionShrinkingStarted(State.Memory.Count()));
        var shrinkCount = 0;
        foreach (var (id, shortTermMemory) in State.Memory.Entries())
        {
            var actionKeys = shortTermMemory.GetActions();
            if (actionKeys.Count < 1) continue;
            foreach (var key in actionKeys)
            {
                shortTermMemory.RemoveAction(key);
                ExecuteRun();
                if (State.EvidenceRecorder.Failed())
                {
                    State.Docket.Record(new ActionRemoved(id, key));
                    shrinkCount++;
                }
                else
                {
                    shortTermMemory.StoreAction(key);
                }
            }
            State.Docket.Record(new ActionShrinkRunEnded(State.Memory.Count()));
        }
        State.Docket.Record(new ActionShrinkingEnded(State.Memory.Count()));
        return shrinkCount;
    }

    private void ExecuteRun()
    {
        State.Docket.Record(new ActionShrinkRunStarted(State.Memory.Count()));
        State.Memory.LongTermMemory.Reset();
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.Memory.Numbers)
        {
            PerformExecution(executionNumber);
        }
        State.Docket.Record(new ActionShrinkRunEnded(State.Memory.Count()));
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.Memory.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
