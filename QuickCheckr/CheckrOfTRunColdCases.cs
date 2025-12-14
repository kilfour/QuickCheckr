using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class CheckrOfTRunColdCases
{
    public static void ReviewColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, false);

    public static void CloseResolvedColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, true);

    private static void Check(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure, bool closeResolved)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        vault.Check(checkr, config, closeResolved);
    }
}
