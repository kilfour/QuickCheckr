namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

public class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
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
                i--;
            }
        }
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        var executionId = State.StartReplay();
        while (executionId > 0)
        {
            if (executionId != current)
                Check(State);
            executionId = State.Memory.NextExecution();
        }
    }
}
