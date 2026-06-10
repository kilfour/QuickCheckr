using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;


/// <summary>
/// Provides helpers for executing a Checkr script and controlling how often it is run.  
/// Use these extensions to run a case once, repeatedly, or with custom configuration.
/// </summary>
public static class CheckrOfTRun
{
    /// <summary>
    /// Runs the Checkr with full control over runs, executions, and configuration.  
    /// Throws a <see cref="FalsifiableException"/> as soon as a failing minimal case is found.  
    /// Use for complete control over test exploration and shrinking behaviour.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        int? seed = null)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default())) with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, runs, executionsPerRun, config, Microtome.Empty);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return findings;
    }

    /// <summary>
    /// Runs the Checkr once with a single execution.  
    /// Use for the simplest possible run when no configuration or repetition is needed.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun());

    /// <summary>
    /// Runs the Checkr once with a single execution and applies additional configuration.  
    /// Use when you want to adjust shrinking, diagnostics, or other settings for a minimal run.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun(), configure);

    /// <summary>
    /// Runs the Checkr multiple times, each consisting of a single execution.  
    /// Use when exploring a larger input space while keeping individual runs isolated.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check, RunCount runs)
        => Run(check, runs, 1.ExecutionsPerRun());

    /// <summary>
    /// Runs the Checkr multiple times with configuration applied.  
    /// Use to combine repeated exploration with tuned settings such as shrinking or diagnostics.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check, RunCount runs, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, runs, 1.ExecutionsPerRun(), configure);

    /// <summary>
    /// Runs the Checkr once but performs multiple executions within that run.  
    /// Use for stateful systems where behaviour depends on repeated steps on the same state.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check, ExecutionCount executionsPerRun)
        => Run(check, 1.Runs(), executionsPerRun);

    /// <summary>
    /// Runs the Checkr once with multiple executions and configuration applied.  
    /// Use when testing stateful behaviour that needs both repetition and custom settings.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(this CheckrOf<Case> check, ExecutionCount executionsPerRun, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), executionsPerRun, configure);

    /// <summary>
    /// Runs the Checkr with the specified number of runs and executions per run.  
    /// Use to explore both pure input variation and multi-step behaviour.
    /// </summary>
    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> check,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(check, runs, executionsPerRun, _ => CheckrConfig.Default());

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> check,
        int seed,
        ExecutionCount executionsPerRun)
            => Run(check, 1.Runs(), executionsPerRun, _ => CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> check,
        int seed,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), executionsPerRun, configure, seed);

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> check,
        int seed)
            => Run(check, 1.Runs(), 1.ExecutionsPerRun(), _ => CheckrConfig.Default(), seed);

    [StackTraceHidden]
    public static IRecord Run(
        this CheckrOf<Case> check,
        int seed,
        Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun(), configure, seed);

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
