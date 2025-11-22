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
        State.LongTermMemory.Reset();
        State.EvidenceRecorder.StartReplay();
        using (State.ShortTermMemories.GetMemory(executionId).ScopedSwap(key, value))
        {
            foreach (var id in State.ShortTermMemories.Numbers)
            {
                State.ShortTermMemories.GotoExecution(id);
                Check(State);
            }
        }
        return State.EvidenceRecorder.Failed();
    }

}