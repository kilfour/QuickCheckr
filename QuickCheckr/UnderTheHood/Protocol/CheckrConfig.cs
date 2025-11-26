using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Configuration;

public record CheckrConfig
{
    public required int? Seed { get; init; }
    public required Flow<IFiling>? Diagnose { get; init; }
    public required bool NoShrinking { get; init; }
    public required bool ShrinkingActions { get; init; }
    public required string FileAs { get; init; } = string.Empty;

    public static CheckrConfig Default()
    {
        return
            new CheckrConfig()
            {
                Seed = null,
                Diagnose = null,
                NoShrinking = false,
                ShrinkingActions = false,
                FileAs = string.Empty
            };
    }
}
