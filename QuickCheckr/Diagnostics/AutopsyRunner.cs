using System.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Diagnostics;

public static class AutopsyRunner
{
    [StackTraceHidden]
    public static CheckrOf<Case> Autopsy(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun,
        AutopsyProbe autopsyProbe)
        => checkr.Autopsy(seed, executionsPerRun, config => config, autopsyProbe);

    [StackTraceHidden]
    internal static CheckrOf<Case> Autopsy(
        this CheckrOf<Case> checkr,
        int seed,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        AutopsyProbe autopsyProbe)
    {
        var config = configure(CheckrConfig.Default()) with { Seed = seed };
        var findings = CaseOfficer.Investigate(checkr, 1.Runs(), executionsPerRun, config, autopsyProbe);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return checkr;
    }

    [StackTraceHidden]
    public static ConfiguredCheckr Autopsy(
        this ConfiguredCheckr configuredCheckr,
        int seed,
        ExecutionCount executionsPerRun,
        AutopsyProbe autopsyProbe)
    {
        var config = configuredCheckr.Config with { Seed = seed };
        var findings = CaseOfficer.Investigate(configuredCheckr.Checkr, 1.Runs(), executionsPerRun, config, autopsyProbe);
        SpokesPerson.Announce(config, findings, executionsPerRun);
        return configuredCheckr;
    }
}
