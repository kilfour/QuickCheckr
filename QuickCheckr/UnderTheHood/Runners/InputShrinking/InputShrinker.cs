using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new InputShrinkingStarted(State.ShortTermMemory.Count()));
        var shrinkCount = 0;
        foreach (var executionId in State.ShortTermMemory.Numbers)
        {
            State.ShortTermMemory.GotoExecution(executionId);
            var inputs = State.ShortTermMemory.GetInputs();
            foreach (var (key, value) in inputs)
            {
                State.Docket.Record(new InputShrinkRunStarted(executionId, key));
                ShrinkStrategyPicker.Input(State, Check, executionId, key, value, key);
                State.Docket.Record(new InputShrinkRunEnded(executionId, key));
            }
        }
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
