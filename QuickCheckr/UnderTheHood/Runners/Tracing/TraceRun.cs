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
                await ScriptExecution.RunAsync(Check, State).ConfigureAwait(false);
            }
        }
        finally
        {
            try { State.Memory.DisposeStash(); }
            finally
            {
                State.Memory.IsConfigRun = false;
                State.Memory.Tracing = false;
            }
        }
    }
}
