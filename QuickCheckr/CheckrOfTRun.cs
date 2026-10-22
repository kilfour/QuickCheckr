using System.Diagnostics;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>
/// Provides entry points for running Checkr scripts synchronously.
/// Use to control the number of runs, executions per run, and the seed used to reproduce a failure.
/// </summary>
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
        var config = checkrConfig with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, runs, executionsPerRun, config, AutopsyProbe.Empty);
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
        var config = configuredCheckr.Config with { Seed = seed };
        var findings = CaseOfficer.Investigate(configuredCheckr.Checkr, runs, executionsPerRun, config, AutopsyProbe.Empty);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return configuredCheckr;
    }

    /// <summary>
    /// Runs the Checkr once with one execution.
    /// Uses the default configuration. Use for a single execution of the script.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr)
        => Run(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    /// <summary>
    /// Runs the configured Checkr once with one execution.
    /// Uses the existing configuration. Use for a single execution of the script.
    /// </summary>
    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr)
        => Run(configuredCheckr, 1.Runs(), 1.ExecutionsPerRun(), null);


    /// <summary>
    /// Runs the Checkr for the specified number of runs, with one execution per run.
    /// Uses the default configuration. Use to explore multiple independent input sets.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr, RunCount runs)
        => Run(checkr, runs, 1.ExecutionsPerRun(), CheckrConfig.Default(), null);

    /// <summary>
    /// Runs the configured Checkr for the specified number of runs, with one execution per run.
    /// Uses the existing configuration. Use to explore multiple independent input sets.
    /// </summary>
    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr, RunCount runs)
        => Run(configuredCheckr, runs, 1.ExecutionsPerRun(), null);


    /// <summary>
    /// Runs the Checkr once with the specified number of executions.
    /// Uses the default configuration. Use to explore behaviour across repeated executions within a run.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(this CheckrOf<Case> checkr, ExecutionCount executionsPerRun)
        => Run(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), null);

    /// <summary>
    /// Runs the configured Checkr once with the specified number of executions.
    /// Uses the existing configuration. Use to explore behaviour across repeated executions within a run.
    /// </summary>
    [StackTraceHidden]
    public static ConfiguredCheckr Run(this ConfiguredCheckr configuredCheckr, ExecutionCount executionsPerRun)
        => Run(configuredCheckr, 1.Runs(), executionsPerRun, null);


    /// <summary>
    /// Runs the Checkr for the specified number of runs and executions per run.
    /// Uses the default configuration. Use to explore different inputs and sequences of steps across runs.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(checkr, runs, executionsPerRun, CheckrConfig.Default(), null);

    /// <summary>
    /// Runs the configured Checkr for the specified number of runs and executions per run.
    /// Uses the existing configuration. Use to explore different inputs and sequences of steps across runs.
    /// </summary>
    [StackTraceHidden]
    public static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(configuredCheckr, runs, executionsPerRun, null);

    /// <summary>
    /// Runs the Checkr once with the specified number of executions using the supplied seed.
    /// Uses the default configuration. Use the supplied seed to reproduce a previous run.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun)
            => Run(checkr, 1.Runs(), executionsPerRun, CheckrConfig.Default(), seed);

    /// <summary>
    /// Runs the configured Checkr once with the specified number of executions using the supplied seed.
    /// Uses the existing configuration. Use the supplied seed to reproduce a previous run.
    /// </summary>
    [StackTraceHidden]
    public static ConfiguredCheckr Run(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun)
        => Run(configuredCheckr, 1.Runs(), executionsPerRun, seed);

    /// <summary>
    /// Runs the Checkr once with one execution using the supplied seed.
    /// Uses the default configuration. Use the supplied seed to reproduce a previous run.
    /// </summary>
    [StackTraceHidden]
    public static CheckrOf<Case> Run(
        this CheckrOf<Case> checkr,
        int seed)
            => Run(checkr, 1.Runs(), 1.ExecutionsPerRun(), CheckrConfig.Default(), seed);

    /// <summary>
    /// Runs the configured Checkr once with one execution using the supplied seed.
    /// Uses the existing configuration. Use the supplied seed to reproduce a previous run.
    /// </summary>
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
