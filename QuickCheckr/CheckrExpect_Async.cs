using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Expect(
        string key,
        Func<ExpectationContext, Task<ExpectationResult>> evaluation)
        => Expect(key, (context, _) => evaluation(context));

    public static CheckrOf<Case> Expect(
        string key,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluation)
        => Evaluate.ExpectationAsync(key, evaluation, true);

    /// <summary>Awaits an expectation only when it is active in the current run or replay.</summary>
    public static CheckrOf<Case> Expect(string key, Func<Task<bool>> evaluation)
        => Expect(key, async context =>
            await evaluation().ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    /// <summary>Awaits an expectation with the run's cancellation token.</summary>
    public static CheckrOf<Case> Expect(string key, Func<CancellationToken, Task<bool>> evaluation)
        => Expect(key, async (context, token) =>
            await evaluation(token).ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());
}
