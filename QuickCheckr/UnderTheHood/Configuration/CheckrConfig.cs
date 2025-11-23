namespace QuickCheckr.UnderTheHood.Configuration;

public record CheckrConfig
{
    public required int? Seed { get; init; }
    public required bool Diagnose { get; init; }
    public required bool NoShrinking { get; init; }
    public required bool ShrinkingActions { get; init; }

    public static CheckrConfig Default()
    {
        return
            new CheckrConfig()
            {
                Seed = null,
                Diagnose = false,
                NoShrinking = false,
                ShrinkingActions = false
            };
    }
}
