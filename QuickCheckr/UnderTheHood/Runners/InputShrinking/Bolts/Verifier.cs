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
        try
        {
            State.Memory.LongTermMemory.Reset();
            State.EvidenceRecorder.StartReplay();
            using (State.Memory.ShortTermMemory.GetMemory(executionId).ScopedSwap(key, value))
            {
                foreach (var id in State.Memory.ShortTermMemory.Numbers)
                {
                    State.Memory.ShortTermMemory.GotoExecution(id);
                    Check(State);
                }
            }
        }
        catch (Exception exception)
        {
            State.EvidenceRecorder.RecordFailure(exception);
        }
        return State.EvidenceRecorder.Failed();
    }
}