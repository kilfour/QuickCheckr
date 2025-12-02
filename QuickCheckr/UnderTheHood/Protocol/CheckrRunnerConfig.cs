using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
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
        EvidenceMode = config.EvidenceMode;
    }

    public int? Seed { get; init; }
    public ManOfWealthAndTaste ManOfWealthAndTaste { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public string FileAs { get; init; }
    public EvidenceMode EvidenceMode { get; init; }
}
