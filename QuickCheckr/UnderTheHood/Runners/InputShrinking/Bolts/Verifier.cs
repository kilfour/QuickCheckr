using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public abstract record ReplayOutcome
{
    public sealed record FailurePreserved : ReplayOutcome;
    public sealed record Passed : ReplayOutcome;
    public sealed record CandidateRejected(Exception Exception) : ReplayOutcome;
}

public class Verifier(CheckrState State,
    CheckrOf<Case> Check)
{
    public virtual bool RunPassed(int executionId, string key, object value)
        => Run(executionId, key, value) is ReplayOutcome.Passed;

    public virtual bool RunFailed(int executionId, string key, object value)
        => Run(executionId, key, value) is ReplayOutcome.FailurePreserved;

    public virtual ReplayOutcome Run(int executionId, string key, object value)
    {
        State.StartReplay();
        IDisposable disposable;
        try
        {
            disposable = State.Memory.ScopedSwap(executionId, key, value);
        }
        catch (CandidateRejectedException exception)
        {
            State.Pathologist.Inform(
                "CandidateRejected",
                key,
                exception.Rejection.GetType().Name);
            return new ReplayOutcome.CandidateRejected(exception.Rejection);
        }

        using (disposable)
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                Check(State);
            }
            State.Memory.DisposeStash();
        }

        return State.EvidenceRecorder.Failed()
            ? new ReplayOutcome.FailurePreserved()
            : new ReplayOutcome.Passed();
    }

    public virtual bool TestimonyPassed(int executionId, string key, object value)
        => Testimony(executionId, key, value) is ReplayOutcome.Passed;

    public virtual bool TestimonyFailed(int executionId, string key, object value)
        => Testimony(executionId, key, value) is ReplayOutcome.FailurePreserved;

    public virtual ReplayOutcome Testimony(int executionId, string key, object value)
    {
        State.StartReplay();
        IDisposable disposable;
        try
        {
            disposable = State.Memory.WithTestimony(executionId, key, value);
        }
        catch (CandidateRejectedException exception)
        {
            State.Pathologist.Inform(
                "CandidateRejected",
                key,
                exception.Rejection.GetType().Name);
            return new ReplayOutcome.CandidateRejected(exception.Rejection);
        }

        using (disposable)
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                Check(State);
            }
            State.Memory.DisposeStash();
        }
        return State.EvidenceRecorder.Failed()
            ? new ReplayOutcome.FailurePreserved()
            : new ReplayOutcome.Passed();
    }
}
