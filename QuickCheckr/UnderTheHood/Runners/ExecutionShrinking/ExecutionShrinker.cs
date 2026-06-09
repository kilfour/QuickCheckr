namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        using var _ = State.Pathologist.Examine("ExecutionShrinking");
        var shrinkCount = 0;
        int passShrinkCount;
        int passNumber = 1;
        do
        {
            passShrinkCount = OnePass(passNumber);
            passNumber++;
            shrinkCount += passShrinkCount;
        } while (passShrinkCount != 0);
        return shrinkCount;
    }

    public int OnePass(int passNumber)
    {
        using var _ = State.Pathologist.Examine("Pass", passNumber);
        var shrinkCount = 0;
        for (var i = 0; i < State.Memory.Numbers.Count && State.Memory.Numbers.Count > 1; i++)
        {
            var id = State.Memory.Numbers[i];
            using var __ = State.Pathologist.Examine("Checking", id);
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                State.Pathologist.Inform("FailedAndRemoved");
                shrinkCount++;
                State.Memory.Remove(id);
                i--;
            }
        }
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        using (State.Memory.ScopedExecutionRemoval(current))
        {
            var executionId = State.StartReplay();
            while (executionId > 0)
            {
                using var _ = State.Pathologist.Examine("Executing", executionId);
                Check(State);
                executionId = State.Memory.NextExecution();
            }
        }
    }
}
