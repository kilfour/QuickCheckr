namespace QuickCheckr.UnderTheHood.Runners.Tracing;

internal sealed class FinalTraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public async ValueTask RunAsync(bool trace = true)
    {
        using var _ = State.Pathologist.Examine("FinalTraceRun");
        State.StartReplay();
        State.Memory.FinalTrace = trace;
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
            finally { State.Memory.FinalTrace = false; }
        }
        State.EvidenceRecorder.RefreshFailureMessages();
    }
}
