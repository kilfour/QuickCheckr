namespace QuickCheckr.UnderTheHood.Runners.Tracing;

internal sealed class TraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public void Run(bool trace)
    {
        using var _ = State.Pathologist.Examine("TraceRun");
        State.StartReplay();
        State.Memory.Tracing = trace;
        State.Memory.IsConfigRun = true;
        foreach (var id in State.Memory.Numbers)
        {
            State.Memory.GotoExecution(id);
            Check(State);
        }
        State.Memory.DisposeStash();
        State.Memory.IsConfigRun = false;
        State.Memory.Tracing = false;
    }
}
