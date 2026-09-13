using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

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
                WarningLevel = WarningLevel.Important,
                MaxStepsPerReducer = 1024
            };
    }

    /// <summary>
    /// Controls how shrinking behaves for this run.  
    /// Use to opt out, into or customise shrinking strategies.
    /// </summary>
    public required ShrinkMode ShrinkMode { get; init; }

    public required ReportMode ReportMode { get; init; }

    public required WarningLevel WarningLevel { get; init; }

    /// <summary>
    /// Label under which this run is filed.  
    /// Used to persist results for later retrieval.
    /// </summary>
    public required string FileAs { get; init; }

    public required ICustodian Custodian { get; init; }

    public ITranscribe Clerk { get; init; } =
        CourtClerk.Default();

    public required int MaxStepsPerReducer { get; init; }
}
