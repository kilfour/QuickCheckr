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
                State.CancellationToken.ThrowIfCancellationRequested();
                await Check(State).ConfigureAwait(false);
                State.CancellationToken.ThrowIfCancellationRequested();
                if (State.RunStopped || State.EvidenceRecorder.Failed())
                    return;
            }
        }
        finally
        {
            await State.Memory.DisposeStashAsync().ConfigureAwait(false);
        }
    }
}
