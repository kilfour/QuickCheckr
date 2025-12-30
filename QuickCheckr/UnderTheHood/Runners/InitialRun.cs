namespace QuickCheckr.UnderTheHood.Runners;

public class InitialRun(CheckrState State, CheckrOf<Case> Check)
{
    public void Run(int numberOfExecutions)
    {
        for (int j = 0; j < numberOfExecutions; j++)
        {
            State.Memory.StoringShrinkers = true;
            State.Memory.StartNewExecution();
            Check(State);
            State.Memory.StoringShrinkers = false;
            if (State.EvidenceRecorder.Failed()) return;
        }

    }
}