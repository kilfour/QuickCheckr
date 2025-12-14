using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.Protocol;

/// <summary>
/// Defines the configuration applied when running a Checkr.  
/// Use to control seeding, shrinking, diagnostics, and how evidence is recorded.
/// </summary>
public record CheckrConfig
{
    /// <summary>
    /// Returns the default configuration for a Checkr run.  
    /// Use as a baseline when applying custom configuration with <c>a => a with { ... }</c>.
    /// </summary>
    public static CheckrConfig Default()
    {
        return
            new CheckrConfig()
            {
                ShrinkMode = ShrinkMode.Default,
                DeliberationPolicy = null,
                FileAs = string.Empty,
                Custodian = Custodian.Default,
                Investigations = []
            };
    }

    public required Func<CaseFile, CaseFile, CaseFile>? DeliberationPolicy { get; init; }

    /// <summary>
    /// Controls how shrinking behaves for this run.  
    /// Use to opt out, into or customise shrinking strategies.
    /// </summary>
    public required ShrinkMode ShrinkMode { get; init; }

    /// <summary>
    /// Label under which this run is filed.  
    /// Used to persist results for later retrieval.
    /// </summary>
    public required string FileAs { get; init; }

    public required Custodian Custodian { get; init; }

    /// <summary>
    /// The set of enabled investigation modes.  
    /// Use to surface additional diagnostics such as shrinking clues or exception traces.
    /// </summary>
    public required Investigation[] Investigations { get; init; }
}
