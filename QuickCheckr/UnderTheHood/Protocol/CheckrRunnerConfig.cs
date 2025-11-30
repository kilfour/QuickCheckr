using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Protocol;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Signals = [.. config.Investigations.Select(a => Signal.From(a.Procedure).SetArtery(a.Officer))];
        ShrinkingActions = config.ShrinkingActions;
        NoShrinking = config.NoShrinking;
        FileAs = config.FileAs;
    }

    public int? Seed { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public bool NoShrinking { get; init; }
    public bool ShrinkingActions { get; init; }
    public string FileAs { get; init; }
}
