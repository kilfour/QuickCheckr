using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Protocol;

public record Investigation(IArtery Officer, Flow<IFiling> Procedure);

public record CheckrConfig
{
    public required int? Seed { get; init; }
    public required Investigation[] Investigations { get; init; }
    public required bool NoShrinking { get; init; }
    public required bool ShrinkingActions { get; init; }
    public required string FileAs { get; init; } = string.Empty;

    public static CheckrConfig Default()
    {
        return
            new CheckrConfig()
            {
                Seed = null,
                Investigations = [],
                NoShrinking = false,
                ShrinkingActions = false,
                FileAs = string.Empty
            };
    }
}
