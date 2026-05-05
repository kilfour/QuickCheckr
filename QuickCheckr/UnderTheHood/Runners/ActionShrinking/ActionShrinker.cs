namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

public class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.Memory.Entries())
        {
            var actionKeys = shortTermMemory.GetActions();
            if (actionKeys.Count < 1) continue;
            foreach (var key in actionKeys)
            {
                shortTermMemory.RemoveAction(key);
                ExecuteRun();
                if (State.EvidenceRecorder.Failed())
                {
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