namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class Verifier(CheckrState State,
    CheckrOf<Case> Check)
{
    public virtual bool RunPassed(int executionId, string key, object value)
        => !RunFailed(executionId, key, value);

    public virtual bool RunFailed(int executionId, string key, object value)
    {
        State.StartReplay();
        var disposable = State.Memory.ScopedSwap(executionId, key, value);
        if (disposable is not null)
        {
            using (disposable)
            {
                foreach (var id in State.Memory.Numbers)
                {
                    State.Memory.GotoExecution(id);
                    Check(State);
                }
            }
        }
        return State.EvidenceRecorder.Failed();
    }

    public virtual bool TestimonyPassed(int executionId, string key, object value)
        => !TestimonyFailed(executionId, key, value);

    public virtual bool TestimonyFailed(int executionId, string key, object value)
    {
        State.StartReplay();
        using (State.Memory.WithTestimony(executionId, key, value))
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