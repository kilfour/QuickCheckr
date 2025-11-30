using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Protocol;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Signals = [.. config.Investigations.Select(a => Signal.From(a.Procedure).SetArtery(a.Officer()))];
        ShrinkMode = config.ShrinkMode;
        FileAs = config.FileAs;
    }

    public int? Seed { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public string FileAs { get; init; }
}
