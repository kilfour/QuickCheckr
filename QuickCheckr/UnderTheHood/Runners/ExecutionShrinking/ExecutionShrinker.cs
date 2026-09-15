using QuickCheckr.Diagnostics.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;

internal sealed class ExecutionShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public async ValueTask<int> RunAsync()
    {
        using var _ = State.Pathologist.Examine(AutopsyPath.ExecutionShrinking);
        var shrinkCount = 0;
        int passShrinkCount;
        int passNumber = 1;
        do
        {
            passShrinkCount = await OnePassAsync(passNumber).ConfigureAwait(false);
            passNumber++;
            shrinkCount += passShrinkCount;
        } while (passShrinkCount != 0);
        return shrinkCount;
    }

    public async ValueTask<int> OnePassAsync(int passNumber)
    {
        using var _ = State.Pathologist.Examine(AutopsyPath.Pass(passNumber));
        var shrinkCount = 0;
        for (var i = 0; i < State.Memory.Numbers.Count && State.Memory.Numbers.Count > 1; i++)
        {
            var id = State.Memory.Numbers[i];
            using var __ = State.Pathologist.Examine(AutopsyPath.Checking(id));
            await ExecuteRunAsync(id).ConfigureAwait(false);
            if (State.EvidenceRecorder.Failed())
            {
                State.Pathologist.Inform(AutopsyPath.FailedAndRemoved);
                shrinkCount++;
                State.Memory.Remove(id);
                i--;
            }
        }
        return shrinkCount;
    }

    private async ValueTask ExecuteRunAsync(int current)
    {
        using (State.Memory.ScopedExecutionRemoval(current))
        {
            try
            {
                var executionId = State.StartReplay();
                while (executionId > 0)
                {
                    using var _ = State.Pathologist.Examine(AutopsyPath.Executing(executionId));
                    State.CancellationToken.ThrowIfCancellationRequested();
                    await Check(State).ConfigureAwait(false);
                    State.CancellationToken.ThrowIfCancellationRequested();
                    executionId = State.Memory.NextExecution();
                }
            }
            finally
            {
                await State.Memory.DisposeStashAsync().ConfigureAwait(false);
            }
        }
    }
}
