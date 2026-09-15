using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Performs a side-effecting operation without recording it as an action.  
    /// Use when the behaviour is required for the test but should not appear in the final report.
    /// </summary>
    public static CheckrOf<Case> Perform(Action action) =>
        state => TheCatcher.InTheCheckr(state, () => { action(); return CheckrResult.CaseOnly(state); });


    /// <summary>Invokes and awaits helper work afresh on each execution and replay.</summary>
    public static CheckrOf<Case> Perform(Func<Task> action) => Perform(_ => action());

    /// <summary>Performs asynchronous helper work using the run's cancellation token.</summary>
    public static CheckrOf<Case> Perform(Func<CancellationToken, Task> action) =>
        state => TheCatcher.InTheCheckrAsync(state, async () =>
        {
            await action(state.CancellationToken).ConfigureAwait(false);
            return CheckrResult.CaseOnly(state);
        });

}
