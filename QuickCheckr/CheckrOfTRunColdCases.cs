using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTRunColdCases
{
    [StackTraceHidden]
    public static void ReviewColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, false);

    [StackTraceHidden]
    public static void CloseResolvedColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, true);

    [StackTraceHidden]
    private static void Check(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure, bool closeResolved)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var caseFiles = vault.Check(checkr, config, closeResolved);
        // config.Custodian.FileRecord(ReportPath.Main(config), new Findings().Add([.. caseFiles]), config.Clerk.File);
    }
}
