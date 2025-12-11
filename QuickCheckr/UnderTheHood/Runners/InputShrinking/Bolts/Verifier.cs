using QuickCheckr.UnderTheHood.Docketing;

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
        State.StartReplay();
        using (State.Memory.ScopedSwap(executionId, key, value))
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                Check(State);
            }
        }
        return State.EvidenceRecorder.Failed();
    }
}