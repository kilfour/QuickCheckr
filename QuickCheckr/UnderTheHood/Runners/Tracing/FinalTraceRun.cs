namespace QuickCheckr.UnderTheHood.Runners.Tracing;

public class FinalTraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public void Run()
    {
        using var _ = State.Pathologist.Examine("FinalTraceRun");
        State.StartReplay();
        State.Memory.FinalTrace = true;
        foreach (var id in State.Memory.Numbers)
        {
            State.Memory.GotoExecution(id);
            Check(State);
        }
        State.Memory.FinalTrace = false;
    }
}
