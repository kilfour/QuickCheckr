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
    public static CaseFile Run(this CheckrOf<Case> check, RunCount runs, ExecutionCount executionsPerRun)
    {
        var signal = Signal.From(flow).SetArtery(FileLog.Write());

        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var caseFile = new CheckrRunner(check, signal).Run(executionsPerRun.NumberOfExecutions);
            //var caseFile = new CheckrRunner(check, null!).Run(executionsPerRun.NumberOfExecutions);
            // state.GetPassedSpecCount(passedSpecCount);
            if (caseFile.HasVerdict())
                // {
                //     vault.AddSeed(state.Seed, executionCount.NumberOfExecutions);
                //     //vaultMessages.Add($"Seed {seed}: added to vault.");
                throw new FalsifiableException(caseFile); //FileIt(caseFile)
            // }
        }

        return CaseFile.Empty();
    }

    public record RunCount(int NumberOfRuns);
    public record ExecutionCount(int NumberOfExecutions);
    public static RunCount Runs(this int numberOfRuns) => new(numberOfRuns);
    public static ExecutionCount ExecutionsPerRun(this int numberOfExecutions) => new(numberOfExecutions);
}
