using QuickCheckr;
using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Configuration;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Signal = config.Diagnose ? QuickPulse.Signal.From(flow).SetArtery(FileLog.Write()) : null;
        ShrinkingActions = config.ShrinkingActions;
        NoShrinking = config.NoShrinking;
    }
    public int? Seed { get; init; }
    public Signal<IFiling>? Signal { get; init; }
    public bool NoShrinking { get; init; }
    public bool ShrinkingActions { get; init; }
    private static readonly Flow<IFiling> flow =
        from filing in Pulse.Start<IFiling>()
        from trace in Pulse.Trace(filing)
        select filing;
}
