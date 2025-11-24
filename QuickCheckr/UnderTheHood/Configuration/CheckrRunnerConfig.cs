using QuickCheckr;
using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Show;

namespace QuickCheckr.UnderTheHood.Configuration;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Signal = config.Diagnose == null ? null : QuickPulse.Signal.From(config.Diagnose).SetArtery(FileLog.Write());
        ShrinkingActions = config.ShrinkingActions;
        NoShrinking = config.NoShrinking;
    }
    public int? Seed { get; init; }
    public Signal<IFiling>? Signal { get; init; }
    public bool NoShrinking { get; init; }
    public bool ShrinkingActions { get; init; }


}
