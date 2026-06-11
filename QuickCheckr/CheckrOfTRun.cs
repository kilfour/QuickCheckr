using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class CheckrOfTRun
{
    [StackTraceHidden]
    private static IRecord Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrConfig checkrConfig,
        int? seed = null)
    {
        var config = new CheckrRunnerConfig(checkrConfig) with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, runs, executionsPerRun, config, Microtome.Empty);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return findings;
    }

    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> checkr)
        => Run(checkr, 1.Runs(), 1.ExecutionsPerRun());

    [StackTraceHidden]
    public static IRecord Run(this ConfiguredCheckr configuredCheckr)
        => Run(configuredCheckr.Checkr, 1.Runs(), 1.ExecutionsPerRun(), configuredCheckr.Config);


    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> checkr, RunCount runs)
        => Run(checkr, runs, 1.ExecutionsPerRun());

    [StackTraceHidden]
    public static IRecord Run(this ConfiguredCheckr configuredCheckr, RunCount runs)
        => Run(configuredCheckr.Checkr, runs, 1.ExecutionsPerRun(), configuredCheckr.Config);


    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> checkr, ExecutionCount executionsPerRun)
        => Run(checkr, 1.Runs(), executionsPerRun);

    [StackTraceHidden]
    public static IRecord Run(this ConfiguredCheckr configuredCheckr, ExecutionCount executionsPerRun)
        => Run(configuredCheckr.Checkr, 1.Runs(), executionsPerRun, configuredCheckr.Config);


    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(checkr, runs, executionsPerRun, CheckrConfig.Default());

    [StackTraceHidden]
    public static IRecord Run(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(configuredCheckr.Checkr, runs, executionsPerRun, configuredCheckr.Config);

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun)
            => Run(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static IRecord Run(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun)
        => Run(configuredCheckr.Checkr, 1.Runs(), executionsPerRun, configuredCheckr.Config, seed);

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> checkr,
        int seed)
            => Run(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static IRecord Run(
        this ConfiguredCheckr configuredCheckr,
        int seed)
        => Run(configuredCheckr.Checkr, 1.Runs(), 1.ExecutionsPerRun(), configuredCheckr.Config, seed);

    /// <summary>
    /// Converts an integer into a <see cref="RunCount"/>.  
    /// Use for specifying how many independent runs the Checkr should perform.
    /// </summary>
    public static RunCount Runs(this int numberOfRuns) => new(numberOfRuns);

    /// <summary>
    /// Converts an integer into an <see cref="ExecutionCount"/>.  
    /// Use for specifying how many executions occur within each run.
    /// </summary>
    public static ExecutionCount ExecutionsPerRun(this int numberOfExecutions) => new(numberOfExecutions);
}
