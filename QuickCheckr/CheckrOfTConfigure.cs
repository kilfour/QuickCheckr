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

    public static ConfiguredCheckr FileAs(
        this CheckrOf<Case> checkr,
        string name)
            => Configure(checkr, a => a with { FileAs = name });

    public static ConfiguredCheckr FileAs(
        this ConfiguredCheckr configuredCheckr,
        string name)
            => Configure(configuredCheckr, a => a with { FileAs = name });
}
