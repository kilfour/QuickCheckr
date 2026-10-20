using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<bool> evaluation)
        => Expect(key, _ => evaluation() ? ExpectationResult.Passed() : ExpectationResult.Failed());

    public static CheckrOf<Case> Expect(this string key, Func<ExpectationContext, ExpectationResult> evaluation)
        => state =>
            state.IsThisTheRunsLastExecution()
            ? Evaluate.Expectation(key, evaluation, true)(state)
            : CheckrResult.CaseOnly(state);

    public static CheckrOf<Case> Expect(
        this string key,
        Func<ExpectationContext, Task<ExpectationResult>> evaluation)
        => Expect(key, (context, _) => evaluation(context));

    public static CheckrOf<Case> Expect(
        this string key,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluation)
        => state => state.IsThisTheRunsLastExecution()
            ? Evaluate.ExpectationAsync(key, evaluation, true)(state)
            : CheckrResult.CaseOnly(state);

    public static CheckrOf<Case> Expect(this string key, Func<Task<bool>> condition)
        => Expect(key, async context =>
            await condition().ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    public static CheckrOf<Case> Expect(this string key, Func<CancellationToken, Task<bool>> condition)
        => Expect(key, async (context, token) =>
            await condition(token).ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());
}

