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
        ManOfWealthAndTaste = config.ManOfWealthAndTaste;
        Signals = [.. config.Investigations.Select(a =>
            Signal.From(
                Pulse.Prime(() => config.ManOfWealthAndTaste)
                    .Then(a.Procedure))
                    .SetArtery(a.Officer()))];
        ShrinkMode = config.ShrinkMode;
        FileAs = config.FileAs;
        Reporter = config.Reporter;
        EvidenceMode = config.EvidenceMode;
    }

    public int? Seed { get; private set; }
    public ManOfWealthAndTaste ManOfWealthAndTaste { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public string FileAs { get; init; }
    public EvidenceMode EvidenceMode { get; init; }
    public Func<string, IArtery> Reporter { get; init; }
    internal void SetSeed(int seed) => Seed = seed;
}
