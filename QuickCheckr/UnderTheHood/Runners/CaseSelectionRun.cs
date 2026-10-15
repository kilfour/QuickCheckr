namespace QuickCheckr.UnderTheHood.Runners;

internal sealed class CaseSelectionRun(
    CheckrState State,
    CheckrOf<Case> Check,
    bool Deliberate,
    bool Distinguish,
    bool RejectCases)
{
    public async ValueTask RunAsync()
    {
        using var _ = State.Pathologist.Examine("CaseSelectionRun");
        State.StartReplay();
        State.Memory.ClearCaseSelection();
        State.Memory.Deliberating = Deliberate;
        State.Memory.Distinguishing = Distinguish;
        State.Memory.RejectingCases = RejectCases;
        try
        {
            foreach (var id in State.Memory.Numbers)
            {
                State.Memory.GotoExecution(id);
                State.CancellationToken.ThrowIfCancellationRequested();
                await Check(State).ConfigureAwait(false);
                State.CancellationToken.ThrowIfCancellationRequested();
                if (State.ExecutionAborted) break;
            }
        }
        finally
        {
            try { await State.Memory.DisposeStashAsync().ConfigureAwait(false); }
            finally
            {
                State.Memory.Deliberating = false;
                State.Memory.Distinguishing = false;
                State.Memory.RejectingCases = false;
            }
        }
    }
}
