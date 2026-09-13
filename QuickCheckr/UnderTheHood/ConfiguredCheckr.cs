using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Couples a Checkr script with the configuration used to run it.
/// </summary>
public sealed record ConfiguredCheckr
{
    internal ConfiguredCheckr(CheckrOf<Case> checkr, CheckrConfig config)
    {
        Checkr = checkr;
        Config = config;
    }

    internal CheckrOf<Case> Checkr { get; }
    internal CheckrConfig Config { get; init; }
}
