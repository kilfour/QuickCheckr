using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class CheckrOfTConfigure
{
    public static ConfiguredCheckr Configure(
        this CheckrOf<Case> checkr,
        Func<CheckrConfig, CheckrConfig> configure)
            => new(checkr, configure(CheckrConfig.Default()));

    public static ConfiguredCheckr Configure(
        this ConfiguredCheckr configuredCheckr,
        Func<CheckrConfig, CheckrConfig> configure)
            => configuredCheckr with { Config = configure(configuredCheckr.Config) };
}
