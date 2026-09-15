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
                await ScriptExecution.RunAsync(Check, State).ConfigureAwait(false);
            }
        }
        finally
        {
            try { State.Memory.DisposeStash(); }
            finally { State.Memory.FinalTrace = false; }
        }
    }
}
