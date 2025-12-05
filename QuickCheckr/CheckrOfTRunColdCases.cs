using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Protocol;

namespace QuickCheckr
{
    public static class CheckrOfTRunColdCases
    {
        // ReviewColdCases
        // ReopenColdCases
        // ResolveColdCases
        // CloseResolvedColdCases
        // InvestigateColdCases
        public static void ReviewColdCases(
            this CheckrOf<Case> checkr,
            Func<CheckrConfig, CheckrConfig> configure)
        {
            var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
            var vault = Vault.From(config);
            vault.Check(checkr, config, false);
        }
    }
}
