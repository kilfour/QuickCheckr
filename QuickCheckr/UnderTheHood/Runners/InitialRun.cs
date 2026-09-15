namespace QuickCheckr.UnderTheHood.Runners;

internal sealed class InitialRun(CheckrState State, CheckrOf<Case> Check)
{
    public async ValueTask RunAsync(int numberOfExecutions)
    {
        using var _ = State.Pathologist.Examine("InitialRun");
        try
        {
            for (int j = 0; j < numberOfExecutions; j++)
            {
                State.Memory.StartNewExecution();
                await ScriptExecution.RunAsync(Check, State).ConfigureAwait(false);
                if (State.EvidenceRecorder.Failed())
                    return;
            }
        }
        finally
        {
            State.Memory.DisposeStash();
        }
    }
}
