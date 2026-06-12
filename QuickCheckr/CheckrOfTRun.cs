using System.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class CheckrOfTRun
{
    [StackTraceHidden]
    private static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrConfig checkrConfig,
        int? seed)
    {
        var config = new CheckrRunnerConfig(checkrConfig) with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, runs, executionsPerRun, config, Microtome.Empty);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return checkr;
    }

    [StackTraceHidden]
    private static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        int? seed)
    {
        var config = new CheckrRunnerConfig(configuredCheckr.Config) with { Seed = seed };
        var findings = CaseOfficer.Investigate(configuredCheckr.Checkr, runs, executionsPerRun, config, Microtome.Empty);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return configuredCheckr;
    }

    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr)
        => Run(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr)
        => Run(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), null);


    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr, RunCount runs)
        => Run(checkr, runs, 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr, RunCount runs)
        => Run(configuredCheckr, runs, 1.ExecutionsPerRun(), null);


    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr, ExecutionCount executionsPerRun)
        => Run(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr, ExecutionCount executionsPerRun)
        => Run(configuredCheckr, 1.Runs(), executionsPerRun, null);


    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(checkr, runs, executionsPerRun, CheckrConfig.Default(), null);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(configuredCheckr, runs, executionsPerRun, null);

    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun)
            => Run(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun)
        => Run(configuredCheckr, 1.Runs(), executionsPerRun, seed);

    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        int seed)
            => Run(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        int seed)
        => Run(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), seed);

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
