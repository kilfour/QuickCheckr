namespace QuickCheckr.UnderTheHood.Runners.Tracing;

public class TraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public void Run(bool trace)
    {
        State.StartReplay();
        State.Memory.Tracing = trace;
        State.Memory.IsConfigRun = true;
        foreach (var id in State.Memory.Numbers)
        {
            State.Memory.GotoExecution(id);
            Check(State);
        }
        State.Memory.IsConfigRun = false;
        State.Memory.Tracing = false;
    }
}
