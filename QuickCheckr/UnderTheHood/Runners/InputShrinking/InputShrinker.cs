using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new InputShrinkingStarted(State.ShortTermMemory.Count()));
        var shrinkCount = 0;
        // for (var i = 0; i < State.ShortTermMemory.Numbers.Count && State.ShortTermMemory.Numbers.Count > 1; i++)
        // {
        //     var id = State.ShortTermMemory.Numbers[i];
        //     State.Docket.Record(new InputShrinkRunStarted(State.ShortTermMemory.Count()));
        //     ExecuteRun(id);
        //     if (State.EvidenceRecorder.Failed())
        //     {
        //         shrinkCount++;
        //         State.ShortTermMemory.Remove(id);
        //         i--;
        //         // State.Docket.Record(new InputShrunk(key));
        //     }
        //     State.Docket.Record(new InputShrinkRunEnded(State.ShortTermMemory.Count()));
        // }
        State.Docket.Record(new InputShrinkingEnded(State.ShortTermMemory.Count()));
        return shrinkCount;
    }

    private void ExecuteRun(int current)
    {
        State.EvidenceRecorder.StartReplay();
        foreach (var executionNumber in State.ShortTermMemory.Numbers)
        {
            if (executionNumber != current)
                PerformExecution(executionNumber);
        }
    }

    private void PerformExecution(int executionNumber)
    {
        State.Docket.Record(new ExecutionStarted(executionNumber));
        State.ShortTermMemory.GotoExecution(executionNumber);
        Check(State);
        State.Docket.Record(new ExecutionFinished(executionNumber));
    }
}
