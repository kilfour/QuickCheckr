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
        //try
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
        }
        // catch (Exception exception)
        // {
        //     // is this the only problem ?
        //     State.Docket.Record(new EvidenceFailure(exception));
        //     State.EvidenceRecorder.RecordFailure(exception);
        // }
        return State.EvidenceRecorder.Failed();
    }
}