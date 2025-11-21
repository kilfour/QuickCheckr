using System.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr;

public static class CheckrOfTRun
{
    private static readonly Flow<IFiling> flow =
        from filing in Pulse.Start<IFiling>()
        from trace in Pulse.Trace(filing)
        select filing;

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun());

    [StackTraceHidden]
    public static CaseFile Run(this CheckrOf<Case> check, Func<CheckrConfig, CheckrConfig> configure)
        => Run(check, 1.Runs(), 1.ExecutionsPerRun(), configure);

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
        var config = configure(CheckrConfig.Default());
        var signal = config.Diagnose ? Signal.From(flow).SetArtery(FileLog.Write()) : null;
        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var caseFile = new CheckrRunner(check, signal, config.Seed).Run(executionsPerRun.NumberOfExecutions);
            // state.GetPassedSpecCount(passedSpecCount);
            if (caseFile.HasVerdict())
                throw new FalsifiableException(caseFile);
        }
        return CaseFile.Empty();
    }

    public static RunCount Runs(this int numberOfRuns) => new(numberOfRuns);
    public static ExecutionCount ExecutionsPerRun(this int numberOfExecutions) => new(numberOfExecutions);
    public record RunCount(int NumberOfRuns);
    public record ExecutionCount(int NumberOfExecutions);
}
