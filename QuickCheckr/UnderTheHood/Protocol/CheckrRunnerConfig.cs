using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Protocol;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Editor = config.Editor;
        Signals = [.. config.Investigations.Select(a =>
            Signal.From(
                Pulse.Prime(() => config.Editor)
                    .Then(a.Procedure))
                    .SetArtery(a.Officer()))];
        ShrinkMode = config.ShrinkMode;
        FileAs = config.FileAs;
        Custodian = config.Custodian;
    }

    public int? Seed { get; private set; }
    public ManOfWealthAndTaste Editor { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public string FileAs { get; init; }
    public Custodian Custodian { get; init; }
    internal void SetSeed(int seed) => Seed = seed;
}
