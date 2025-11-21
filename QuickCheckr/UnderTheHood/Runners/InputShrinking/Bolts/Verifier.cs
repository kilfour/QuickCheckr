namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class Verifier(CheckrState State,
    CheckrOf<Case> Check)
{
    public virtual bool RunPassed(int executionId, string key, object value)
    {
        return !RunFailed(executionId, key, value);
    }

    public virtual bool RunFailed(int executionId, string key, object value)
    {
        //State.Memory.ResetRunScopedInputs();
        //var runNumber = State.CurrentExecutionNumber;
        State.EvidenceRecorder.StartReplay();
        using (State.ShortTermMemory.ScopedSwap(executionId, key, value))
        {
            foreach (var id in State.ShortTermMemory.Numbers)
            {
                State.ShortTermMemory.GotoExecution(id);
                Check(State);
            }
        }
        return State.EvidenceRecorder.Failed();
    }

}