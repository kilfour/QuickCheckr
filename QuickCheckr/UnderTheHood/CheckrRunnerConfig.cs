using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;
using QuickPulse.Show;

namespace QuickCheckr.UnderTheHood;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        FileAs = config.FileAs;
        Custodian = config.Custodian;
        ShrinkMode = config.ShrinkMode;
        ReportMode = config.ReportMode;
        DeliberationPolicy = config.DeliberationPolicy;
        DeliberationTarget = config.DeliberationTarget;
        Signals = [.. config.InternalAffairs.Select(a =>
            Signal.From(
                Pulse.Prime(() => Please.AllowMe().ToUseNonLinearTime())
                    .Then(a.Procedure))
                .SetArtery(a.Officer()))];

    }

    public int? Seed { get; set; } = null;
    public string FileAs { get; init; }
    public Custodian Custodian { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public ReportMode ReportMode { get; init; }
    public Func<ICaseFileSummary, int>? DeliberationPolicy { get; init; }
    public int? DeliberationTarget { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
}
