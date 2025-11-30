using System.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Protocol;

namespace QuickCheckr;


public static class CheckrOfTRun
{
    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun());

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun(), configure);

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, RunCount runs)
        => Run(check, runs, 1.ExecutionsPerRun());

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, RunCount runs, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, runs, 1.ExecutionsPerRun(), configure);

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, ExecutionCount executionsPerRun)
        => Run(check, 1.Runs(), executionsPerRun);

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, ExecutionCount executionsPerRun, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), executionsPerRun, configure);

    [StackTraceHidden]
    public static CaseFile Run(
        this CheckrOf<Case> check,
        RunCount runs,
        ExecutionCount executionsPerRun)
            => Run(check, runs, executionsPerRun, _ => CheckrConfig.Default());

    [StackTraceHidden]
    public static CaseFile Run(
        this CheckrOf<Case> check,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var caseFile = new CheckrRunner(check, config, executionsPerRun.NumberOfExecutions).Run();
            if (caseFile.HasVerdict())
            {
                throw new FalsifiableException(caseFile);
            }
        }
        return CaseFile.Empty();
    }

    public static RunCount Runs(this int numberOfRuns) => new(numberOfRuns);
    public static ExecutionCount ExecutionsPerRun(this int numberOfExecutions) => new(numberOfExecutions);
    public record RunCount(int NumberOfRuns);
    public record ExecutionCount(int NumberOfExecutions);
}
