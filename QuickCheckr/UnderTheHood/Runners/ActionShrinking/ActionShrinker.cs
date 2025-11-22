using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

public class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new ActionShrinkingStarted(State.ShortTermMemories.Count()));
        var shrinkCount = 0;
        foreach (var (id, shortTermMemory) in State.ShortTermMemories.Entries())
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
            State.Docket.Record(new ActionShrinkRunEnded(State.ShortTermMemories.Count()));
        }
        // for (var i = 0; i < State.ShortTermMemories.Numbers.Count; i++)
        // {
        //     var id = State.ShortTermMemories.Numbers[i];
        //     State.ShortTermMemories.GotoExecution(id);
        //     var oldKeys = State.ShortTermMemories.GetActions();
        //     if (oldKeys.Count < 1) continue;
        //     foreach (var key in oldKeys)
        //     {
        //         State.ShortTermMemories.RemoveAction(key);
        //         ExecuteRun();
        //         if (State.EvidenceRecorder.Failed())
        //         {
        //             State.Docket.Record(new ActionRemoved(id, key));
        //             shrinkCount++;
        //         }
        //         else
        //         {
        //             State.ShortTermMemories.GotoExecution(id);
        //             State.ShortTermMemories.StoreAction(key);
        //         }
        //     }
        //     State.Docket.Record(new ActionShrinkRunEnded(State.ShortTermMemories.Count()));
        // }
        State.Docket.Record(new ActionShrinkingEnded(State.ShortTermMemories.Count()));
        return shrinkCount;
    }

    private void ExecuteRun()
    {
        State.Docket.Record(new ActionShrinkRunStarted(State.ShortTermMemories.Count()));
        State.LongTermMemory.Reset();
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.ShortTermMemories.Numbers)
        {
            PerformExecution(executionNumber);
        }
        State.Docket.Record(new ActionShrinkRunEnded(State.ShortTermMemories.Count()));
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemories.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
