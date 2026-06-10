namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

public class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        using var _ = State.Pathologist.Examine("ActionShrinking");
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.Memory.Entries())
        {
            using var __ = State.Pathologist.Examine("Execution", executionId);
            var actionKeys = shortTermMemory.GetActions();
            if (actionKeys.Count < 1) continue;
            foreach (var key in actionKeys)
            {
                using var ___ = State.Pathologist.Examine("Action", key);
                shortTermMemory.RemoveAction(key);
                ExecuteRun();
                if (State.EvidenceRecorder.Failed())
                {
                    State.Pathologist.Inform("FailedAndRemoved");
                    shrinkCount++;
                }
                else
                {
                    shortTermMemory.StoreAction(key);
                }
            }
        }
        return shrinkCount;
    }

    private void ExecuteRun()
    {
        var executionId = State.StartReplay();
        while (executionId > 0)
        {
            Check(State);
            executionId = State.Memory.NextExecution();
        }
    }
}