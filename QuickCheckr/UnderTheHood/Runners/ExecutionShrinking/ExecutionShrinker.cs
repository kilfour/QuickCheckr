using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        var shrinkCount = 0;
        int passShrinkCount;
        do
        {
            passShrinkCount = OnePass();
            shrinkCount += passShrinkCount;
        } while (passShrinkCount != 0);
        return shrinkCount;
    }

    public int OnePass()
    {
        var shrinkCount = 0;
        for (var i = 0; i < State.Memory.Numbers.Count && State.Memory.Numbers.Count > 1; i++)
        {
            var id = State.Memory.Numbers[i];
            ExecuteRun(id);
            if (State.EvidenceRecorder.Failed())
            {
                shrinkCount++;
                State.Memory.Remove(id);
                State.Docket.Record(new ExecutionRemoved(id));
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
                Check(State);
                executionId = State.Memory.NextExecution();
            }
        }
    }
}
