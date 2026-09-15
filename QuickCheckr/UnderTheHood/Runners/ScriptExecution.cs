namespace QuickCheckr.UnderTheHood.Runners;

internal static class ScriptExecution
{
    // Temporary boundary until CheckrOf<T> itself becomes awaitable in step 3.
    public static ValueTask<CheckrResult<Case>> RunAsync(CheckrOf<Case> check, CheckrState state)
        => new(check(state));
}
