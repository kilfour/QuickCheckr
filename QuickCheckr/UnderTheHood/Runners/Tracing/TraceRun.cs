namespace QuickCheckr.UnderTheHood.Runners.Tracing;

internal sealed class TraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public async ValueTask RunAsync(bool trace)
    {
        using var _ = State.Pathologist.Examine("TraceRun");
        State.StartReplay();
        State.Memory.Tracing = trace;
        State.Memory.IsConfigRun = true;
        try
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                State.CancellationToken.ThrowIfCancellationRequested();
                await Check(State).ConfigureAwait(false);
                State.CancellationToken.ThrowIfCancellationRequested();
                if (State.ExecutionAborted) break;
            }
        }
        finally
        {
            try { await State.Memory.DisposeStashAsync().ConfigureAwait(false); }
            finally
            {
                State.Memory.IsConfigRun = false;
                State.Memory.Tracing = false;
            }
        }
    }
}
