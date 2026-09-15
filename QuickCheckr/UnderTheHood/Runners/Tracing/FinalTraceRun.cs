namespace QuickCheckr.UnderTheHood.Runners.Tracing;

internal sealed class FinalTraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public async ValueTask RunAsync()
    {
        using var _ = State.Pathologist.Examine("FinalTraceRun");
        State.StartReplay();
        State.Memory.FinalTrace = true;
        try
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                State.CancellationToken.ThrowIfCancellationRequested();
                await Check(State).ConfigureAwait(false);
                State.CancellationToken.ThrowIfCancellationRequested();
            }
        }
        finally
        {
            try { await State.Memory.DisposeStashAsync().ConfigureAwait(false); }
            finally { State.Memory.FinalTrace = false; }
        }
    }
}
