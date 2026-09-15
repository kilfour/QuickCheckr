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
/// Cancellation is cooperative; callbacks must complete before replay state is released.
/// Use RunAsync for suspending operations; synchronous Run remains a compatibility API.
/// </remarks>
public static class CheckrOfTRunAsync
{
    [StackTraceHidden]
    private static async Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrConfig checkrConfig,
        int? seed,
        CancellationToken cancellationToken)
    {
        var config = new CheckrRunnerConfig(checkrConfig) with { Seed = seed, CancellationToken = cancellationToken };
        var findings = await CaseOfficer.InvestigateAsync(checkr, runs, executionsPerRun, config, AutopsyProbe.Empty).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return checkr;
    }

    [StackTraceHidden]
    private static async Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        int? seed,
        CancellationToken cancellationToken)
    {
        var config = new CheckrRunnerConfig(configuredCheckr.Config) with { Seed = seed, CancellationToken = cancellationToken };
        var findings = await CaseOfficer.InvestigateAsync(configuredCheckr.Checkr, runs, executionsPerRun, config, AutopsyProbe.Empty).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return configuredCheckr;
    }

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr, CancellationToken cancellationToken = default)
        => RunAsync(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), null, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr, CancellationToken cancellationToken = default)
        => RunAsync(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), null, cancellationToken);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr, RunCount runs, CancellationToken cancellationToken = default)
        => RunAsync(checkr, runs, 1.ExecutionsPerRun(), CheckrConfig.Default(), null, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr, RunCount runs, CancellationToken cancellationToken = default)
        => RunAsync(configuredCheckr, runs, 1.ExecutionsPerRun(), null, cancellationToken);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(this CheckrOf<Case> checkr, ExecutionCount executionsPerRun, CancellationToken cancellationToken = default)
        => RunAsync(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), null, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(this ConfiguredCheckr configuredCheckr, ExecutionCount executionsPerRun, CancellationToken cancellationToken = default)
        => RunAsync(configuredCheckr, 1.Runs(), executionsPerRun, null, cancellationToken);


    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CancellationToken cancellationToken = default)
            => RunAsync(checkr, runs, executionsPerRun, CheckrConfig.Default(), null, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CancellationToken cancellationToken = default)
            => RunAsync(configuredCheckr, runs, executionsPerRun, null, cancellationToken);

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun,
        CancellationToken cancellationToken = default)
            => RunAsync(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), seed, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun,
        CancellationToken cancellationToken = default)
        => RunAsync(configuredCheckr, 1.Runs(), executionsPerRun, seed, cancellationToken);

    [StackTraceHidden]
    public static Task<CheckrOf<Case>> RunAsync(
        this CheckrOf<Case> checkr,
        int seed,
        CancellationToken cancellationToken = default)
            => RunAsync(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), seed, cancellationToken);

    [StackTraceHidden]
    public static Task<ConfiguredCheckr> RunAsync(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        CancellationToken cancellationToken = default)
        => RunAsync(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), seed, cancellationToken);

}
