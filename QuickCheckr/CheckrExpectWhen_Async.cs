using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<bool> predicate,
        Func<ExpectationContext, Task<ExpectationResult>> evaluation)
        => Evaluate.ExpectationWhenAsync(
            key,
            _ => Task.FromResult(predicate()),
            (context, _) => evaluation(context),
            true);

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<bool> predicate,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluation)
        => Evaluate.ExpectationWhenAsync(
            key,
            _ => Task.FromResult(predicate()),
            evaluation,
            true);

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<Task<bool>> predicate,
        Func<ExpectationContext, Task<ExpectationResult>> evaluation)
        => Evaluate.ExpectationWhenAsync(
            key,
            _ => predicate(),
            (context, _) => evaluation(context),
            true);

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<CancellationToken, Task<bool>> predicate,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluation)
        => Evaluate.ExpectationWhenAsync(key, predicate, evaluation, true);

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<bool> predicate,
        Func<Task<bool>> evaluation)
        => ExpectWhen(key, predicate, async context =>
            await evaluation().ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<bool> predicate,
        Func<CancellationToken, Task<bool>> evaluation)
        => ExpectWhen(key, predicate, async (context, token) =>
            await evaluation(token).ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<Task<bool>> predicate,
        Func<Task<bool>> evaluation)
        => ExpectWhen(key, predicate, async context =>
            await evaluation().ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    public static CheckrOf<Case> ExpectWhen(
        string key,
        Func<CancellationToken, Task<bool>> predicate,
        Func<CancellationToken, Task<bool>> evaluation)
        => ExpectWhen(key, predicate, async (context, token) =>
            await evaluation(token).ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());
}
