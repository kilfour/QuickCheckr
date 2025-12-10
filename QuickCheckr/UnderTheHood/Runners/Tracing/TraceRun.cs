namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class TraceRun(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public void Run()
    {
        State.StartReplay();
        State.Memory.Tracing = true;
        foreach (var id in State.Memory.Numbers)
        {
            State.Memory.GotoExecution(id);
            Check(State);
        }
        State.Memory.Tracing = false;
    }
}
