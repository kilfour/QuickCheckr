using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Protocol;

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
                Seed = null,
                ManOfWealthAndTaste = new ManOfWealthAndTaste().ToInline<List<int>>(),
                Investigations = [],
                ShrinkMode = ShrinkMode.Default,
                FileAs = string.Empty,
                EvidenceMode = EvidenceMode.Ignore,
                Custodian = Custodian.Default
            };
    }

    /// <summary>
    /// Optional fixed seed for reproducible runs.  
    /// Use when you want deterministic behaviour for debugging or CI.
    /// </summary>
    public required int? Seed { get; init; }

    /// <summary>
    /// Configures how values are rendered in diagnostic output via QuickPulse.Show.  
    /// Use to adjust formatting, verbosity, or custom display rules for traced or reported data.
    /// </summary>
    public required ManOfWealthAndTaste ManOfWealthAndTaste { get; init; }

    /// <summary>
    /// The set of enabled investigation modes.  
    /// Use to surface additional diagnostics such as shrinking clues or exception traces.
    /// </summary>
    public required Investigation[] Investigations { get; init; }

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

    /// <summary>
    /// Controls how QuickCheckr uses the evidence vault during a run.
    /// Use to ignore historical failures, replay them for regression checking,
    /// or automatically purge seeds that no longer reproduce a failure.
    /// </summary>
    public required EvidenceMode EvidenceMode { get; init; }

    public required Custodian Custodian { get; init; }
}
