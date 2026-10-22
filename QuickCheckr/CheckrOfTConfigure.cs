using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static class CheckrOfTConfigure
{
    internal static ConfiguredCheckr Configure(
        this CheckrOf<Case> checkr,
        Func<CheckrConfig, CheckrConfig> configure)
            => new(checkr, configure(CheckrConfig.Default()));

    internal static ConfiguredCheckr Configure(
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

    /// <summary>Sets the shrinking strategies used for this run.</summary>
    public static ConfiguredCheckr ShrinkMode(
        this CheckrOf<Case> checkr,
        ShrinkMode mode)
            => Configure(checkr, config => config with { ShrinkMode = mode });

    /// <summary>Sets the shrinking strategies used for this run.</summary>
    public static ConfiguredCheckr ShrinkMode(
        this ConfiguredCheckr configuredCheckr,
        ShrinkMode mode)
            => Configure(configuredCheckr, config => config with { ShrinkMode = mode });

    /// <summary>Sets which execution traces are reported.</summary>
    public static ConfiguredCheckr ReportMode(
        this CheckrOf<Case> checkr,
        ReportMode mode)
            => Configure(checkr, config => config with { ReportMode = mode });

    /// <summary>Sets which execution traces are reported.</summary>
    public static ConfiguredCheckr ReportMode(
        this ConfiguredCheckr configuredCheckr,
        ReportMode mode)
            => Configure(configuredCheckr, config => config with { ReportMode = mode });

    /// <summary>Sets the verbosity of warnings.</summary>
    public static ConfiguredCheckr WarningLevel(
        this CheckrOf<Case> checkr,
        WarningLevel level)
            => Configure(checkr, config => config with { WarningLevel = level });

    /// <summary>Sets the verbosity of warnings.</summary>
    public static ConfiguredCheckr WarningLevel(
        this ConfiguredCheckr configuredCheckr,
        WarningLevel level)
            => Configure(configuredCheckr, config => config with { WarningLevel = level });

    /// <summary>Sets the maximum number of steps attempted by each reducer.</summary>
    public static ConfiguredCheckr MaxStepsPerReducer(
        this CheckrOf<Case> checkr,
        int steps)
            => Configure(checkr, config => config with { MaxStepsPerReducer = steps });

    /// <summary>Sets the maximum number of steps attempted by each reducer.</summary>
    public static ConfiguredCheckr MaxStepsPerReducer(
        this ConfiguredCheckr configuredCheckr,
        int steps)
            => Configure(configuredCheckr, config => config with { MaxStepsPerReducer = steps });

    /// <summary>Sets the custodian used to store and retrieve evidence.</summary>
    public static ConfiguredCheckr Custodian(
        this CheckrOf<Case> checkr,
        ICustodian custodian)
            => Configure(checkr, config => config with { Custodian = custodian });

    /// <summary>Sets the custodian used to store and retrieve evidence.</summary>
    public static ConfiguredCheckr Custodian(
        this ConfiguredCheckr configuredCheckr,
        ICustodian custodian)
            => Configure(configuredCheckr, config => config with { Custodian = custodian });

    /// <summary>Sets the clerk used to transcribe reports.</summary>
    public static ConfiguredCheckr Clerk(
        this CheckrOf<Case> checkr,
        ITranscribe clerk)
            => Configure(checkr, config => config with { Clerk = clerk });

    /// <summary>Sets the clerk used to transcribe reports.</summary>
    public static ConfiguredCheckr Clerk(
        this ConfiguredCheckr configuredCheckr,
        ITranscribe clerk)
            => Configure(configuredCheckr, config => config with { Clerk = clerk });
}
