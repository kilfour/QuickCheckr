using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Protocol;

public record Investigation(Func<IArtery> Officer, Flow<IFiling> Procedure);


public record CheckrConfig
{
    public required int? Seed { get; init; }
    public required Investigation[] Investigations { get; init; }
    public required ShrinkMode ShrinkMode { get; init; }
    public required string FileAs { get; init; } = string.Empty;
    public required EvidenceMode EvidenceMode { get; init; }

    public static CheckrConfig Default()
    {
        return
            new CheckrConfig()
            {
                Seed = null,
                Investigations = [],
                ShrinkMode = ShrinkMode.Default,
                FileAs = string.Empty,
                EvidenceMode = EvidenceMode.Ignore
            };
    }
}
