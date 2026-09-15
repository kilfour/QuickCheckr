namespace QuickCheckr.UnderTheHood.Runners.ActionShrinking;

internal sealed class ActionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public async ValueTask<int> RunAsync()
    {
        using var _ = State.Pathologist.Examine("ActionShrinking");
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.Memory.Entries())
        {
            using var __ = State.Pathologist.Examine("Execution", executionId);
            var actionKeys = shortTermMemory.GetShrinkableActions();
            if (actionKeys.Count < 1) continue;
            foreach (var key in actionKeys)
            {
                using var ___ = State.Pathologist.Examine("Checking", "Action", key);
                shortTermMemory.RemoveAction(key);
                try
                {
                    await ExecuteRunAsync().ConfigureAwait(false);
                }
                catch
                {
                    shortTermMemory.StoreAction(key);
                    throw;
                }
                if (State.EvidenceRecorder.Failed())
                {
                    State.Pathologist.Inform("FailedAndRemoved");
                    shrinkCount++;
                }
                else
                {
                    State.Pathologist.Inform("Kept");
                    shortTermMemory.StoreAction(key);
                }
            }
        }
        return shrinkCount;
    }

    private async ValueTask ExecuteRunAsync()
    {
        try
        {
            var executionId = State.StartReplay();
            while (executionId > 0)
            {
                await ScriptExecution.RunAsync(Check, State).ConfigureAwait(false);
                executionId = State.Memory.NextExecution();
            }
        }
        finally
        {
            State.Memory.DisposeStash();
        }
    }
}
