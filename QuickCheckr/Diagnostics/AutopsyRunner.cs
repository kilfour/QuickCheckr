using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Diagnostics;

public static class AutopsyRunner
{
    [StackTraceHidden]
    public static IRecord Autopsy(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        List<string> focuses)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default())) with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, 1.Runs(), executionsPerRun, config, focuses);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return findings;
    }
}