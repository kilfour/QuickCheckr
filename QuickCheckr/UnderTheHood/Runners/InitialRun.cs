namespace QuickCheckr.UnderTheHood.Runners;

internal sealed class InitialRun(CheckrState State, CheckrOf<Case> Check)
{
    public void Run(int numberOfExecutions)
    {
        using var _ = State.Pathologist.Examine("InitialRun");
        for (int j = 0; j < numberOfExecutions; j++)
        {
            State.Memory.StartNewExecution();
            Check(State);
            if (State.EvidenceRecorder.Failed())
            {
                State.Memory.DisposeStash();
                return;
            }
        }
        State.Memory.DisposeStash();
    }
}
