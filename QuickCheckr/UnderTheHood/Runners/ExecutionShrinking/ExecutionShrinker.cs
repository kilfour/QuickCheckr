using QuickCheckr.Diagnostics.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        using var _ = State.Pathologist.Examine(AutopsyPath.ExecutionShrinking);
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
        using var _ = State.Pathologist.Examine(AutopsyPath.Pass(passNumber));
        var shrinkCount = 0;
        for (var i = 0; i < State.Memory.Numbers.Count && State.Memory.Numbers.Count > 1; i++)
        {
            var id = State.Memory.Numbers[i];
            using var __ = State.Pathologist.Examine(AutopsyPath.Checking(id));
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                State.Pathologist.Inform(AutopsyPath.FailedAndRemoved);
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
                using var _ = State.Pathologist.Examine(AutopsyPath.Executing(executionId));
                Check(State);
                executionId = State.Memory.NextExecution();
            }
            State.Memory.DisposeStash();
        }
    }
}
