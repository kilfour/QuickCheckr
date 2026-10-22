using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Couples a Checkr script with the configuration used to run it.
/// </summary>
public record ConfiguredCheckr
{
    internal ConfiguredCheckr(CheckrOf<Case> checkr, CheckrConfig config)
    {
        Checkr = checkr;
        Config = config;
    }

    internal CheckrOf<Case> Checkr { get; }
    internal CheckrConfig Config { get; init; }
}

public sealed record PeristentCheckr : ConfiguredCheckr
{
    internal PeristentCheckr(CheckrOf<Case> checkr, CheckrConfig config) : base(checkr, config) { }
}
