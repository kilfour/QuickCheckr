using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Proceedings;

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
                FileAs = string.Empty,
                Custodian = Protocol.Custodian.Default,
                ShrinkMode = ShrinkMode.Default,
                ReportMode = ReportMode.Default,
                DeliberationPolicy = null,
                DeliberationTarget = null,
                InternalAffairs = []
            };
    }

    public required Func<ICaseFileSummary, int>? DeliberationPolicy { get; init; }
    public required int? DeliberationTarget { get; init; }

    /// <summary>
    /// Controls how shrinking behaves for this run.  
    /// Use to opt out, into or customise shrinking strategies.
    /// </summary>
    public required ShrinkMode ShrinkMode { get; init; }

    public required ReportMode ReportMode { get; init; }

    /// <summary>
    /// Label under which this run is filed.  
    /// Used to persist results for later retrieval.
    /// </summary>
    public required string FileAs { get; init; }

    public required ICustodian Custodian { get; init; }

    /// <summary>
    /// The set of enabled InternalAffair modes.  
    /// Use to surface additional diagnostics such as shrinking clues or exception traces.
    /// </summary>
    public required InternalAffair[] InternalAffairs { get; init; }
}
