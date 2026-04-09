namespace QuickCheckr.UnderTheHood.Runners.Tracing;

public class FinalTraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public void Run()
    {
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
