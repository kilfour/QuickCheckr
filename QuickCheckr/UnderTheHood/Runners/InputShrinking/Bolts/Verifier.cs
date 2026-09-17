using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

internal abstract record ReplayOutcome
{
    public sealed record FailurePreserved : ReplayOutcome;
    public sealed record Passed : ReplayOutcome;
    public sealed record CandidateRejected(Exception Exception) : ReplayOutcome;
}

internal class Verifier(CheckrState State,
    CheckrOf<Case> Check)
{
    public virtual async ValueTask<bool> RunPassedAsync(int executionId, string key, object value)
        => await RunAsync(executionId, key, value).ConfigureAwait(false) is ReplayOutcome.Passed;

    public virtual async ValueTask<bool> RunFailedAsync(int executionId, string key, object value)
        => await RunAsync(executionId, key, value).ConfigureAwait(false) is ReplayOutcome.FailurePreserved;

    public virtual async ValueTask<ReplayOutcome> RunAsync(int executionId, string key, object value)
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
            try
            {
                foreach (var id in State.Memory.Numbers)
                {
                    State.Memory.GotoExecution(id);
                    State.CancellationToken.ThrowIfCancellationRequested();
                    await Check(State).ConfigureAwait(false);
                    State.CancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                await State.Memory.DisposeStashAsync().ConfigureAwait(false);
            }
        }

        return GetOutcome();
    }

    public virtual async ValueTask<bool> TestimonyPassedAsync(int executionId, string key, object value)
        => await TestimonyAsync(executionId, key, value).ConfigureAwait(false) is ReplayOutcome.Passed;

    public virtual async ValueTask<bool> TestimonyFailedAsync(int executionId, string key, object value)
        => await TestimonyAsync(executionId, key, value).ConfigureAwait(false) is ReplayOutcome.FailurePreserved;

    public virtual async ValueTask<ReplayOutcome> TestimonyAsync(int executionId, string key, object value)
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
            try
            {
                foreach (var id in State.Memory.Numbers)
                {
                    State.Memory.GotoExecution(id);
                    State.CancellationToken.ThrowIfCancellationRequested();
                    await Check(State).ConfigureAwait(false);
                    State.CancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                await State.Memory.DisposeStashAsync().ConfigureAwait(false);
            }
        }
        return GetOutcome();
    }

    private ReplayOutcome GetOutcome()
    {
        // A different exception blocks the replay; it is not evidence of a passing input.
        if (State.EvidenceRecorder.ReplayException is { } exception)
            return new ReplayOutcome.CandidateRejected(exception);

        return State.EvidenceRecorder.Failed()
            ? new ReplayOutcome.FailurePreserved()
            : new ReplayOutcome.Passed();
    }
}
