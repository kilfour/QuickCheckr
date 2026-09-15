using System.Diagnostics;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>
/// Provides awaitable entry points for running Checkr scripts.
/// </summary>
/// <remarks>
/// Awaits script execution, including replay and shrinking.
/// Existing task-based action helpers still block until the next migration step.
/// </remarks>
public static class CheckrOfTRunAsync
{
    [StackTraceHidden]
    private static async Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrConfig checkrConfig,
        int? seed)
    {
        var config = new CheckrRunnerConfig(checkrConfig) with { Seed = seed };
        var findings = await CaseOfficer.InvestigateAsync(checkr, runs, executionsPerRun, config, AutopsyProbe.Empty).ConfigureAwait(false);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return checkr;
    }

    [StackTraceHidden]
    private static async Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        int? seed)
    {
        var config = new CheckrRunnerConfig(configuredCheckr.Config) with { Seed = seed };
        var findings = await CaseOfficer.InvestigateAsync(configuredCheckr.Checkr, runs, executionsPerRun, config, AutopsyProbe.Empty).ConfigureAwait(false);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return configuredCheckr;
    }

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr)
        => RunAsync(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr)
        => RunAsync(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), null);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr, RunCount runs)
        => RunAsync(checkr, runs, 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr, RunCount runs)
        => RunAsync(configuredCheckr, runs, 1.ExecutionsPerRun(), null);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr, ExecutionCount executionsPerRun)
        => RunAsync(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr, ExecutionCount executionsPerRun)
        => RunAsync(configuredCheckr, 1.Runs(), executionsPerRun, null);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => RunAsync(checkr, runs, executionsPerRun, CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => RunAsync(configuredCheckr, runs, executionsPerRun, null);

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun)
            => RunAsync(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun)
        => RunAsync(configuredCheckr, 1.Runs(), executionsPerRun, seed);

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        int seed)
            => RunAsync(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        int seed)
        => RunAsync(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), seed);

}
